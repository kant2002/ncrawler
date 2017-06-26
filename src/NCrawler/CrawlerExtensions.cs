namespace NCrawler
{
    using System;

    /// <summary>
    /// Helper extensions for crawler.
    /// </summary>
    public static class CrawlerExtensions
    {
        /// <summary>
        /// Start crawl process synchronously.
        /// </summary>
        /// <param name="crawler">Crawler for which start crawling.</param>
        public static void Crawl(this Crawler crawler)
        {
            crawler.CrawlAsync().Wait();
        }

        /// <summary>
        /// Add crawler step process synchronously.
        /// </summary>
        /// <param name="crawler">Crawler for which start crawling.</param>
        /// <param name="uri">Url to crawl</param>
        /// <param name="depth">Depth of the url</param>
        public static void AddStep(this Crawler crawler, Uri uri, int depth)
        {
            crawler.AddStepAsync(uri, depth).Wait();
        }

        /// <summary>
        /// Add crawler step process synchronously.
        /// </summary>
        /// <param name="crawler">Crawler for which start crawling.</param>
        /// <param name="uri">Url to crawl</param>
        /// <param name="depth">Depth of the url</param>
        public static void Crawl(this Crawler crawler, string uri, int depth)
        {
            crawler.AddStep(new Uri(uri), depth);
        }
    }
}
