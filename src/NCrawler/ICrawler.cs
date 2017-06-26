namespace NCrawler
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for the crawler.
    /// </summary>
    public interface ICrawler
    {
        /// <summary>
        /// Queue a new step on the crawler queue
        /// </summary>
        /// <param name="uri">Url to crawl</param>
        /// <param name="depth">Depth of the url</param>
        /// <param name="referrer">Step which the url was located</param>
        /// <param name="properties">Custom properties</param>
        /// <returns>Task which asynchronously add the step to the crawler queue.</returns>
        Task AddStepAsync(Uri uri, int depth, CrawlStep referrer, Dictionary<string, object> properties);

        /// <summary>
        /// Start crawling.
        /// </summary>
        /// <returns>Task which start crawl.</returns>
        Task CrawlAsync();
    }
}
