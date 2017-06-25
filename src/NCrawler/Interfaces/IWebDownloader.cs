using System;
using System.Threading.Tasks;
using NCrawler.Events;
using NCrawler.Services;

namespace NCrawler.Interfaces
{
	public interface IWebDownloader
	{
		TimeSpan? ConnectionTimeout { get; set; }
		uint? DownloadBufferSize { get; set; }
		uint? MaximumContentSize { get; set; }
		uint? MaximumDownloadSizeInRam { get; set; }
		TimeSpan? ReadTimeout { get; set; }
		int? RetryCount { get; set; }
		TimeSpan? RetryWaitDuration { get; set; }
		bool UseCookies { get; set; }
		string UserAgent { get; set; }

        Task<PropertyBag> DownloadAsync(CrawlStep crawlStep, CrawlStep referrer, DownloadMethod method);

        Task<RequestState<T>> DownloadAsync<T>(CrawlStep crawlStep, CrawlStep referrer, DownloadMethod method,
            Func<RequestState<T>, Task> completed, Action<DownloadProgressEventArgs> progress, T state);
    }
}