namespace NCrawler
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Helper extensions for crawler.
    /// </summary>
    public static class CrawlerExtensions
    {
        /// <summary>
        /// Start crawl process synchronously.
        /// </summary>
        /// <param name="crawler">Crawler for which start crawling.</param>
        public static void Crawl(this ICrawler crawler)
        {
            crawler.CrawlAsync().Wait();
        }

        /// <summary>
        /// 	Queue a new step on the crawler queue
        /// </summary>
        /// <param name="uri">url to crawl</param>
        /// <param name="depth">depth of the url</param>
        public static async Task AddStepAsync(this ICrawler crawler, Uri uri, int depth)
        {
            await crawler.AddStepAsync(uri, depth, null, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Add crawler step process synchronously.
        /// </summary>
        /// <param name="crawler">Crawler for which start crawling.</param>
        /// <param name="uri">Url to crawl</param>
        /// <param name="depth">Depth of the url</param>
        public static void AddStep(this ICrawler crawler, Uri uri, int depth)
        {
            crawler.AddStepAsync(uri, depth).Wait();
        }

        /// <summary>
        /// Add crawler step process synchronously.
        /// </summary>
        /// <param name="crawler">Crawler for which start crawling.</param>
        /// <param name="uri">Url to crawl</param>
        /// <param name="depth">Depth of the url</param>
        public static void Crawl(this ICrawler crawler, string uri, int depth)
        {
            crawler.AddStep(new Uri(uri), depth);
        }
    }
}
