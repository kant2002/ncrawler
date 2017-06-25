using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;

using NCrawler.Extensions;
using NCrawler.Interfaces;
using NCrawler.Services;
using NCrawler.Utils;

namespace NCrawler
{
	public partial class Crawler : DisposableBase
	{
		#region Readonly & Static Fields

		protected readonly Uri m_BaseUri;
		private readonly ILifetimeScope m_LifetimeScope;
		private readonly ThreadSafeCounter m_ThreadInUse = new ThreadSafeCounter();
		private long m_VisitedCount;

		#endregion

		#region Fields

		protected ICrawlerHistory m_CrawlerHistory;
		protected ICrawlerQueue m_CrawlerQueue;
		protected ILog m_Logger;
		protected ITaskRunner m_TaskRunner;
		protected Func<IWebDownloader> m_WebDownloaderFactory;

		private bool m_Cancelled;
		private ManualResetEvent m_CrawlCompleteEvent;
		private bool m_CrawlStopped;
		private ICrawlerRules m_CrawlerRules;
		private bool m_Crawling;
		private long m_DownloadErrors;
		private Stopwatch m_Runtime;
		private bool m_OnlyOneCrawlPerInstance;

		#endregion

		#region Constructors

		/// <summary>
		/// 	Constructor for NCrawler
		/// </summary>
		/// <param name = "crawlStart">The url from where the crawler should start</param>
		/// <param name = "pipeline">Pipeline steps</param>
		public Crawler(Uri crawlStart, params IPipelineStep[] pipeline)
		{
			AspectF.Define.
				NotNull(crawlStart, "crawlStart").
				NotNull(pipeline, "pipeline");

            this.m_LifetimeScope = NCrawlerModule.Container.BeginLifetimeScope();
            this.m_BaseUri = crawlStart;
            this.MaximumCrawlDepth = null;
            this.AdhereToRobotRules = true;
            this.MaximumThreadCount = 1;
            this.Pipeline = pipeline;
            this.UriSensitivity = UriComponents.HttpRequestUrl;
            this.MaximumDownloadSizeInRam = 1024*1024;
            this.DownloadBufferSize = 50 * 1024;
		}

        #endregion

        #region Instance Methods

        /// <summary>
        /// 	Start crawl process
        /// </summary>
        public virtual async Task CrawlAsync()
        {
            if (this.m_OnlyOneCrawlPerInstance)
            {
                throw new InvalidOperationException("Crawler instance cannot be reused");
            }

            this.m_OnlyOneCrawlPerInstance = true;

            var parameters = new Parameter[]
                {
                    new TypedParameter(typeof (Uri), this.m_BaseUri),
                    new NamedParameter("crawlStart", this.m_BaseUri),
                    new TypedParameter(typeof (Crawler), this),
                };
            this.m_CrawlerQueue = this.m_LifetimeScope.Resolve<ICrawlerQueue>(parameters);
            parameters = parameters.AddToEnd(new TypedParameter(typeof(ICrawlerQueue), this.m_CrawlerQueue)).ToArray();
            this.m_CrawlerHistory = this.m_LifetimeScope.Resolve<ICrawlerHistory>(parameters);
            parameters = parameters.AddToEnd(new TypedParameter(typeof(ICrawlerHistory), this.m_CrawlerHistory)).ToArray();
            this.m_TaskRunner = this.m_LifetimeScope.Resolve<ITaskRunner>(parameters);
            parameters = parameters.AddToEnd(new TypedParameter(typeof(ITaskRunner), this.m_TaskRunner)).ToArray();
            this.m_Logger = this.m_LifetimeScope.Resolve<ILog>(parameters);
            parameters = parameters.AddToEnd(new TypedParameter(typeof(ILog), this.m_Logger)).ToArray();
            this.m_CrawlerRules = this.m_LifetimeScope.Resolve<ICrawlerRules>(parameters);
            this.m_Logger.Verbose("Crawl started @ {0}", this.m_BaseUri);
            this.m_WebDownloaderFactory = this.m_LifetimeScope.Resolve<Func<IWebDownloader>>();
            using (this.m_CrawlCompleteEvent = new ManualResetEvent(false))
            {
                this.m_Crawling = true;
                this.m_Runtime = Stopwatch.StartNew();

                if (this.m_CrawlerQueue.Count > 0)
                {
                    // Resume enabled
                    ProcessQueue();
                }
                else
                {
                    await AddStepAsync(this.m_BaseUri, 0);
                }

                if (!this.m_CrawlStopped)
                {
                    this.m_CrawlCompleteEvent.WaitOne();
                }

                this.m_Runtime.Stop();
                this.m_Crawling = false;
            }

            if (this.m_Cancelled)
            {
                OnCancelled();
            }

            this.m_Logger.Verbose("Crawl ended @ {0} in {1}", this.m_BaseUri, this.m_Runtime.Elapsed);
            OnCrawlFinished();
        }

        /// <summary>
        /// 	Queue a new step on the crawler queue
        /// </summary>
        /// <param name = "uri">url to crawl</param>
        /// <param name = "depth">depth of the url</param>
        public async Task AddStepAsync(Uri uri, int depth)
        {
            await AddStepAsync(uri, depth, null, null);
        }

        /// <summary>
        /// 	Queue a new step on the crawler queue
        /// </summary>
        /// <param name = "uri">url to crawl</param>
        /// <param name = "depth">depth of the url</param>
        /// <param name = "referrer">Step which the url was located</param>
        /// <param name = "properties">Custom properties</param>
        public async Task AddStepAsync(Uri uri, int depth, CrawlStep referrer, Dictionary<string, object> properties)
        {
            if (!this.m_Crawling)
            {
                throw new InvalidOperationException("Crawler must be running before adding steps");
            }

            if (this.m_CrawlStopped)
            {
                return;
            }

            var allowedReferrer = await this.m_CrawlerRules.IsAllowedUrlAsync(uri, referrer);
            if ((uri.Scheme != "https" && uri.Scheme != "http") || // Only accept http(s) schema
                (this.MaximumCrawlDepth.HasValue && this.MaximumCrawlDepth.Value > 0 && depth >= this.MaximumCrawlDepth.Value) ||
                !allowedReferrer ||
                !this.m_CrawlerHistory.Register(uri.GetUrlKeyString(this.UriSensitivity)))
            {
                if (depth == 0)
                {
                    StopCrawl();
                }

                return;
            }

            // Make new crawl step
            var crawlStep = new CrawlStep(uri, depth)
            {
                IsExternalUrl = this.m_CrawlerRules.IsExternalUrl(uri),
                IsAllowed = true,
            };
            this.m_CrawlerQueue.Push(new CrawlerQueueEntry
            {
                CrawlStep = crawlStep,
                Referrer = referrer,
                Properties = properties
            });
            this.m_Logger.Verbose("Added {0} to queue referred from {1}",
                crawlStep.Uri, referrer.IsNull() ? string.Empty : referrer.Uri.ToString());
            ProcessQueue();
        }

        public void Cancel()
		{
			if (!this.m_Crawling)
			{
				throw new InvalidOperationException("Crawler must be running before cancellation is possible");
			}

            this.m_Logger.Verbose("Cancelled crawler from {0}", this.m_BaseUri);
			if (this.m_Cancelled)
			{
				throw new OperationCanceledException("Already cancelled once");
			}

            this.m_Cancelled = true;
			StopCrawl();
		}

		protected override void Cleanup()
		{
            this.m_LifetimeScope.Dispose();
		}

        private async Task EndDownloadAsync(RequestState<ThreadSafeCounter.ThreadSafeCounterCookie> requestState)
        {
            using (requestState.State)
            {
                if (requestState.Exception != null)
                {
                    OnDownloadException(requestState.Exception, requestState.CrawlStep, requestState.Referrer);
                }

                if (!requestState.PropertyBag.IsNull())
                {
                    requestState.PropertyBag.Referrer = requestState.CrawlStep;

                    // Assign initial properties to propertybag
                    if (!requestState.State.CrawlerQueueEntry.Properties.IsNull())
                    {
                        requestState.State.CrawlerQueueEntry.Properties.
                            ForEach(key => requestState.PropertyBag[key.Key].Value = key.Value);
                    }

                    if (OnAfterDownload(requestState.CrawlStep, requestState.PropertyBag))
                    {
                        // Executes all the pipelines sequentially for each downloaded content
                        // in the crawl process. Used to extract data from content, like which
                        // url's to follow, email addresses, aso.
                        await this.Pipeline.ForEach(pipelineStep => ExecutePipeLineStep(pipelineStep, requestState.PropertyBag));
                    }
                }
            }

            ProcessQueue();
        }

        private async Task ExecutePipeLineStep(IPipelineStep pipelineStep, PropertyBag propertyBag)
		{
			try
			{
				var sw = Stopwatch.StartNew();
                this.m_Logger.Debug("Executing pipeline step {0}", pipelineStep.GetType().Name);
                if (pipelineStep is IPipelineStepWithTimeout stepWithTimeout)
                {
                    this.m_Logger.Debug("Running pipeline step {0} with timeout {1}",
                        pipelineStep.GetType().Name, stepWithTimeout.ProcessorTimeout);
                    this.m_TaskRunner.RunSync(async cancelArgs =>
                        {
                            if (!cancelArgs.Cancel)
                            {
                                await pipelineStep.ProcessAsync(this, propertyBag);
                            }
                        }, stepWithTimeout.ProcessorTimeout);
                }
                else
                {
                    await pipelineStep.ProcessAsync(this, propertyBag);
                }

                this.m_Logger.Debug("Executed pipeline step {0} in {1}", pipelineStep.GetType().Name, sw.Elapsed);
			}
			catch (Exception ex)
			{
				OnProcessorException(propertyBag, ex);
			}
		}

		private void ProcessQueue()
		{
			if (this.ThreadsInUse == 0 && this.WaitingQueueLength == 0)
			{
                this.m_CrawlCompleteEvent.Set();
				return;
			}

			if (this.m_CrawlStopped)
			{
				if (this.ThreadsInUse == 0)
				{
                    this.m_CrawlCompleteEvent.Set();
				}

				return;
			}

			if (this.MaximumCrawlTime.HasValue && this.m_Runtime.Elapsed > this.MaximumCrawlTime.Value)
			{
                this.m_Logger.Verbose("Maximum crawl time({0}) exceeded, cancelling", this.MaximumCrawlTime.Value);
				StopCrawl();
				return;
			}

			if (this.MaximumCrawlCount.HasValue && this.MaximumCrawlCount.Value > 0 &&
                this.MaximumCrawlCount.Value <= Interlocked.Read(ref this.m_VisitedCount))
			{
                this.m_Logger.Verbose("CrawlCount exceeded {0}, cancelling", this.MaximumCrawlCount.Value);
				StopCrawl();
				return;
			}

			while (this.ThreadsInUse < this.MaximumThreadCount && this.WaitingQueueLength > 0)
			{
				StartDownload();
			}
		}

		private void StartDownload()
		{
			var crawlerQueueEntry = this.m_CrawlerQueue.Pop();
			if (crawlerQueueEntry.IsNull() || !OnBeforeDownload(crawlerQueueEntry.CrawlStep))
			{
				return;
			}

			var webDownloader = this.m_WebDownloaderFactory();
			webDownloader.MaximumDownloadSizeInRam = this.MaximumDownloadSizeInRam;
			webDownloader.ConnectionTimeout = this.ConnectionTimeout;
			webDownloader.MaximumContentSize = this.MaximumContentSize;
			webDownloader.DownloadBufferSize = this.DownloadBufferSize;
			webDownloader.UserAgent = this.UserAgent;
			webDownloader.UseCookies = this.UseCookies;
			webDownloader.ReadTimeout = this.ConnectionReadTimeout;
			webDownloader.RetryCount = this.DownloadRetryCount;
			webDownloader.RetryWaitDuration = this.DownloadRetryWaitDuration;
            this.m_Logger.Verbose("Downloading {0}", crawlerQueueEntry.CrawlStep.Uri);
			var threadSafeCounterCookie = this.m_ThreadInUse.EnterCounterScope(crawlerQueueEntry);
			Interlocked.Increment(ref this.m_VisitedCount);
			webDownloader.DownloadAsync(crawlerQueueEntry.CrawlStep, crawlerQueueEntry.Referrer, DownloadMethod.GET,
                this.EndDownloadAsync, this.OnDownloadProgress, threadSafeCounterCookie);
		}

		private void StopCrawl()
		{
			if (this.m_CrawlStopped)
			{
				return;
			}

            this.m_CrawlStopped = true;
			if (this.ThreadsInUse == 0)
			{
                this.m_CrawlCompleteEvent.Set();
				return;
			}
		}

		#endregion
	}
}