namespace NCrawler
{
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
    }
}
