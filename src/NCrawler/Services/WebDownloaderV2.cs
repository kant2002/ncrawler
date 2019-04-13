using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NCrawler.Events;
using NCrawler.Extensions;
using NCrawler.Interfaces;
using NCrawler.Utils;

namespace NCrawler.Services
{
	public enum DownloadMethod
	{
		GET,
		POST,
		HEAD,
	}

	public class WebDownloaderV2 : IWebDownloader
	{
		#region Constants

		private const uint DefaultDownloadBufferSize = 50*1024;

		#endregion

		#region Fields

		private CookieContainer m_CookieContainer;

		#endregion

		#region Instance Properties

		private CookieContainer CookieContainer
		{
			get { return this.m_CookieContainer ?? (this.m_CookieContainer = new CookieContainer()); }
		}

		#endregion

		#region Instance Methods

		/// <summary>
		/// 	Override this to make set custom request properties
		/// </summary>
		/// <param name = "request"></param>
		protected virtual void SetDefaultRequestProperties(HttpClientHandler handler, HttpRequestMessage request)
		{
            handler.AllowAutoRedirect = true;
			request.Headers.UserAgent.ParseAdd(this.UserAgent);
			request.Headers.Accept.ParseAdd("*/*");
			request.Headers.Connection.ParseAdd("Keep-Alive");
            // handler.Pipelined = true;
            if (this.ConnectionTimeout.HasValue)
			{
				//request.Timeout = Convert.ToInt32(ConnectionTimeout.Value.TotalMilliseconds);
			}

			if (this.ReadTimeout.HasValue)
			{
				//request.ReadWriteTimeout = Convert.ToInt32(ReadTimeout.Value.TotalMilliseconds);
			}

			if (this.UseCookies)
			{
                handler.CookieContainer = this.CookieContainer;
			}
		}

		private async Task<RequestState<T>> DownloadAsync<T>(RequestState<T> requestState, Exception exception)
		{
			if (!exception.IsNull() && this.RetryWaitDuration.HasValue)
			{
				Thread.Sleep(this.RetryWaitDuration.Value);
            }

            return await DownloadAsync(requestState).ConfigureAwait(false);
        }

        private async Task<RequestState<T>> DownloadAsync<T>(RequestState<T> requestState)
        {
            if (requestState.Retry-- <= 0)
            {
                requestState.CallComplete(null, new TimeoutException("Connection Timeout"));
                return requestState;
            }

            requestState.Clean();
            var clientHandler = new HttpClientHandler();
            var client = new HttpClient(clientHandler);
            var request = new HttpRequestMessage(requestState.Method, requestState.CrawlStep.Uri);
            requestState.Request = request;
            SetDefaultRequestProperties(clientHandler, request);
            
            Exception unhandledException = null;
            HttpResponseMessage httpResponse = null;
            try
            {
                httpResponse = await client.SendAsync(request).WithTimeout(this.ConnectionTimeout).ConfigureAwait(false);
                var response = httpResponse.Content;
                var downloadBufferSize = this.DownloadBufferSize ?? DefaultDownloadBufferSize;
                var contentLength = response.Headers.ContentLength ?? throw new InvalidOperationException("Content length not specified");
                requestState.ResponseBuffer = new MemoryStreamWithFileBackingStore((int)contentLength,
                    this.MaximumDownloadSizeInRam ?? int.MaxValue,
                    (int)downloadBufferSize);

                // Read the response into a Stream object. 
                var responseStream = await response.ReadAsStreamAsync().WithTimeout(this.ReadTimeout).ConfigureAwait(false);
                await responseStream.CopyToAsync(requestState.ResponseBuffer,
                    (source, dest, exception) =>
                    {
                        if (exception.IsNull())
                        {
                            CallComplete(requestState, httpResponse);
                        }
                        else
                        {
                            // Put delay here in case of error.
                            //DownloadWithRetryAsync(requestState, exception);
                        }
                    },
                    bd =>
                    {
                        requestState.DownloadProgress?.Invoke(new DownloadProgressEventArgs
                        {
                            Referrer = requestState.Referrer,
                            Step = requestState.CrawlStep,
                            BytesReceived = bd,
                            TotalBytesToReceive = (uint)httpResponse.Content.Headers.ContentLength,
                            DownloadTime = requestState.StartTime - DateTime.UtcNow,
                        });
                    },
                    downloadBufferSize, this.MaximumContentSize, this.ReadTimeout).ConfigureAwait(false);
                CallComplete(requestState, httpResponse);
            }
            catch (HttpRequestException)
            {
                CallComplete(requestState, httpResponse);
            }
            catch (Exception e)
            {
                unhandledException = e;
            }

            if (unhandledException != null)
            {
                return await DownloadWithRetryAsync(requestState).ConfigureAwait(false);
            }

            return requestState;
        }

        /// <summary>
        /// Start downloading of the request as continuation for the retry.
        /// </summary>
        /// <typeparam name="T">Type of the state object.</typeparam>
        /// <param name="requestState">Request state.</param>
        /// <returns>Task which return request state on completition.</returns>
        private async Task<RequestState<T>> DownloadWithRetryAsync<T>(RequestState<T> requestState)
        {
            if (this.RetryWaitDuration.HasValue)
            {
                await Task.Delay(this.RetryWaitDuration.Value).ConfigureAwait(false);
            }

            return await DownloadAsync(requestState).ConfigureAwait(false);
        }


        /// <summary>
        /// 	Gets or Sets a value indicating if cookies will be stored.
        /// </summary>
        private async Task<PropertyBag> DownloadInternalSync(CrawlStep crawlStep, CrawlStep referrer, DownloadMethod method)
		{
			PropertyBag result = null;
			Exception ex = null;
			using (var resetEvent = new ManualResetEvent(false))
			{
				await DownloadAsync(crawlStep, referrer, method,
					(RequestState<object> state) =>
						{
							if (state.Exception.IsNull())
							{
								result = state.PropertyBag;
								if (!result.GetResponse.IsNull())
								{
									using (var response = result.GetResponse())
									{
										byte[] data;
										if (response is MemoryStream)
										{
											data = ((MemoryStream) response).ToArray();
										}
										else
										{
											using (var copy = response.CopyToMemory())
											{
												data = copy.ToArray();
											}
										}

										result.GetResponse = () => new MemoryStream(data);
									}
								}
							}
							else
							{
								ex = state.Exception;
							}

							resetEvent.Set();
                            return Task.FromResult(0);
						}, null, null).ConfigureAwait(false);

				resetEvent.WaitOne();
			}

			if (!ex.IsNull())
			{
				throw new Exception("Error write downloading {0}".FormatWith(crawlStep.Uri), ex);
			}

			return result;
		}

		#endregion

		#region IWebDownloader Members

		public int? RetryCount { get; set; }
		public TimeSpan? RetryWaitDuration { get; set; }
		public TimeSpan? ConnectionTimeout { get; set; }
		public uint? MaximumContentSize { get; set; }
		public uint? MaximumDownloadSizeInRam { get; set; }
		public uint? DownloadBufferSize { get; set; }
		public TimeSpan? ReadTimeout { get; set; }
		public bool UseCookies { get; set; }
		public string UserAgent { get; set; }

		public async Task<PropertyBag> DownloadAsync(CrawlStep crawlStep, CrawlStep referrer, DownloadMethod method)
		{
			return await DownloadInternalSync(crawlStep, referrer, method).ConfigureAwait(false);
		}

		public async Task<RequestState<T>> DownloadAsync<T>(CrawlStep crawlStep, CrawlStep referrer, DownloadMethod method,
            Func<RequestState<T>, Task> completed, Action<DownloadProgressEventArgs> progress,
			T state)
		{
			AspectF.Define.
				NotNull(crawlStep, "crawlStep").
				NotNull(completed, "completed");

			if (this.UserAgent.IsNullOrEmpty())
			{
                this.UserAgent = "Mozilla/5.0";
			}

			var requestState = new RequestState<T>
				{
					DownloadTimer = Stopwatch.StartNew(),
					Complete = completed,
					CrawlStep = crawlStep,
					Referrer = referrer,
					State = state,
					DownloadProgress = progress,
					Retry = this.RetryCount.HasValue ? this.RetryCount.Value + 1 : 1,
					Method = ConvertToHttpMethod(method),
				};

			return await this.DownloadAsync(requestState, null).ConfigureAwait(false);
		}
        #endregion

        #region Class Methods

        private static void CallComplete<T>(RequestState<T> requestState, HttpResponseMessage response)
		{
            PropertyBag propertyBag;
			if (response != null)
			{
                propertyBag = new PropertyBag
                {
                    Step = requestState.CrawlStep,
                    CharacterSet = response.Content.Headers.ContentType.CharSet,
                    ContentEncoding = response.Content.Headers.ContentEncoding.FirstOrDefault(),
                    ContentType = response.Content.Headers.ContentType.MediaType,
                    Headers = response.Headers,

                    // Mutually authenticated requests not supported
                    IsMutuallyAuthenticated = false,

                    // We always load data not from cache.
                    IsFromCache = false,
                    LastModified = response.Content.Headers.LastModified,
                    Method = response.RequestMessage.Method.ToString().ToUpperInvariant(),
                    ProtocolVersion = response.RequestMessage.Version,
                    ResponseUri = response.RequestMessage.RequestUri,
                    Server = response.Headers.Server.Select(_ => _.Product?.Name + " " + _.Product?.Version).FirstOrDefault(),
                    StatusCode = response.StatusCode,
                    StatusDescription = response.ReasonPhrase,
                    GetResponse = requestState.ResponseBuffer.IsNull()
                                ? (Func<Stream>)(() => new MemoryStream())
                                : requestState.ResponseBuffer.GetReaderStream,
                    DownloadTime = requestState.DownloadTimer.Elapsed,
                };
			}
			else
			{
                propertyBag = new PropertyBag
                {
                    Step = requestState.CrawlStep,
                    CharacterSet = string.Empty,
                    ContentEncoding = null,
                    ContentType = null,
                    Headers = null,
                    IsMutuallyAuthenticated = false,
                    IsFromCache = false,
                    LastModified = DateTime.Now,
                    Method = string.Empty,
                    ProtocolVersion = null,
                    ResponseUri = null,
                    Server = string.Empty,
                    StatusCode = HttpStatusCode.Forbidden,
                    StatusDescription = string.Empty,
                    GetResponse = requestState.ResponseBuffer.IsNull()
                                ? (Func<Stream>)(() => new MemoryStream())
                                : requestState.ResponseBuffer.GetReaderStream,
                    DownloadTime = requestState.DownloadTimer.Elapsed,
                };
			}

            requestState.CallComplete(propertyBag, null);
        }
        #endregion

        private HttpMethod ConvertToHttpMethod(DownloadMethod method)
        {
            switch (method)
            {
                case DownloadMethod.GET:
                    return HttpMethod.Get;
                case DownloadMethod.HEAD:
                    return HttpMethod.Head;
                case DownloadMethod.POST:
                    return HttpMethod.Post;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}