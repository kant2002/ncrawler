using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NCrawler.Events;
using NCrawler.Interfaces;
using NCrawler.Services;
using NCrawler.Shared.Tests.Properties;

namespace NCrawler.Test.Helpers
{
	public class FakeDownloader : IWebDownloader
	{
		#region IWebDownloader Members

		public Task<PropertyBag> DownloadAsync(CrawlStep crawlStep, CrawlStep referrer = null, DownloadMethod method = DownloadMethod.GET)
		{
			var result = new PropertyBag
			{
				Step = crawlStep,
				CharacterSet = string.Empty,
				ContentEncoding = string.Empty,
				ContentType = "text/html",
				Headers = null,
				IsMutuallyAuthenticated = false,
				IsFromCache = false,
				LastModified = DateTime.UtcNow,
				Method = "GET",
				ProtocolVersion = new Version(3, 0),
				ResponseUri = crawlStep.Uri,
				Server = "N/A",
				StatusCode = HttpStatusCode.OK,
				StatusDescription = "OK",
				GetResponse = () => new MemoryStream(Encoding.UTF8.GetBytes(Resources.ncrawler_codeplex_com)),
				DownloadTime = TimeSpan.FromSeconds(1),
			};

			return Task.FromResult(result);
		}

        public Task<RequestState<T>> DownloadAsync<T>(CrawlStep crawlStep, CrawlStep referrer, DownloadMethod method, Func<RequestState<T>, Task> completed, Action<DownloadProgressEventArgs> progress, T state)
        {
            var result = new RequestState<T>
            {
                StartTime = DateTime.UtcNow,
                Complete = completed,
                CrawlStep = crawlStep,
                Referrer = referrer,
                State = state,
                DownloadProgress = progress,
                Retry = this.RetryCount.HasValue ? this.RetryCount.Value + 1 : 1,
                Method = this.ConvertToHttpMethod(method),
            };
            return Task.Factory.StartNew(() =>
            {
                completed(result);
                return result;
            });
        }

        public TimeSpan? ConnectionTimeout { get; set; }
		public uint? DownloadBufferSize { get; set; }
		public uint? MaximumContentSize { get; set; }
		public uint? MaximumDownloadSizeInRam { get; set; }
		public TimeSpan? ReadTimeout { get; set; }
		public int? RetryCount { get; set; }
		public TimeSpan? RetryWaitDuration { get; set; }
		public bool UseCookies { get; set; }
		public string UserAgent { get; set; }

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