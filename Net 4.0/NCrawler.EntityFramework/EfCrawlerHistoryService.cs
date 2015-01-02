// -----------------------------------------------------------------------
// <copyright file="EfCrawlerHistoryService.cs" company="Andrey Kurdiumov">
// Copyright (c) Andrey Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace NCrawler.EntityFramework
{
    using System;
    using System.Linq;

    /// <summary>
    /// Crawler service which is used to crawl.
    /// </summary>
    public class EfCrawlerHistoryService : NCrawler.Utils.HistoryServiceBase
    {
        /// <summary>
        /// Id of the crawling group.
        /// </summary>
        private readonly int groupId;

        /// <summary>
        /// Flag indicating whether module should resume previous work or not.
        /// </summary>
        private readonly bool resume;

        /// <summary>
        /// Initializes a new instance of the <see cref="EfCrawlerHistoryService"/> class.
        /// </summary>
        /// <param name="uri">Uri from which work is started.</param>
        /// <param name="resume">True to resume work, false otherwise.</param>
        public EfCrawlerHistoryService(Uri uri, bool resume)
        {
            this.resume = resume;
            this.groupId = uri.GetHashCode();

            if (!this.resume)
            {
                this.CleanHistory();
            }
        }

        /// <summary>
        /// Adds history entry with specific key.
        /// </summary>
        /// <param name="key">Key to add to history entry.</param>
        protected override void Add(string key)
        {
            using (var model = new NCrawlerModel())
            {
                var historyEntry = new CrawlHistory();
                historyEntry.Key = key;
                historyEntry.GroupId = this.groupId;
                model.CrawlHistories.Add(historyEntry);
                model.SaveChanges();
            }
        }

        /// <summary>
        /// Performs cleanup of resources.
        /// </summary>
        protected override void Cleanup()
        {
            if (!this.resume)
            {
                this.CleanHistory();
            }

            base.Cleanup();
        }

        /// <summary>
        /// Tests whether item with given key present in the current crawling group.
        /// </summary>
        /// <param name="key">Key of the item to test.</param>
        /// <returns>True if item is present in the history; false otherwise.</returns>
        protected override bool Exists(string key)
        {
            using (var model = new NCrawlerModel())
            {
                return model.CrawlHistories.Where(h => h.GroupId == this.groupId && h.Key == key).Any();
            }
        }

        /// <summary>
        /// Gets count of registered entries.
        /// </summary>
        /// <returns>Count of registered history entries.</returns>
        protected override long GetRegisteredCount()
        {
            using (var model = new NCrawlerModel())
            {
                return model.CrawlHistories.Count(h => h.GroupId == this.groupId);
            }
        }

        /// <summary>
        /// Cleans group history.
        /// </summary>
        private void CleanHistory()
        {
            using (var model = new NCrawlerModel())
            {
                model.Database.ExecuteSqlCommand("DELETE FROM CrawlHistories WHERE GroupId = {0}", this.groupId);
            }
        }
    }
}
