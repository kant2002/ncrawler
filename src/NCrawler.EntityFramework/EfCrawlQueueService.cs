// -----------------------------------------------------------------------
// <copyright file="EfCrawlQueueService.cs" company="Andrey Kurdiumov">
// Copyright (c) Andrey Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace NCrawler.EntityFramework
{
    using System;
    using System.Linq;

    using NCrawler.Extensions;

    /// <summary>
    /// Crawler queue using Entity Framework
    /// </summary>
    public class EfCrawlQueueService : NCrawler.Utils.CrawlerQueueServiceBase
    {
        /// <summary>
        /// Id of the crawling group.
        /// </summary>
        private readonly int groupId;

        /// <summary>
        /// Initializes a new instance of the <see cref="EfCrawlQueueService"/> class.
        /// </summary>
        /// <param name="baseUri">Uri from which work is started.</param>
        /// <param name="resume">True to resume work, false otherwise.</param>
        public EfCrawlQueueService(Uri baseUri, bool resume)
        {
            this.groupId = baseUri.GetHashCode();
            if (!resume)
            {
                this.CleanQueue();
            }
        }
        
        /// <summary>
        /// Get count of items in the queue.
        /// </summary>
        /// <returns>Count of items in the queue.</returns>
        protected override long GetCount()
        {
            using (var model = new NCrawlerModel())
            {
                return model.CrawlQueues.Count(h => h.GroupId == this.groupId);
            }
        }

        /// <summary>
        /// Pop item from the queue.
        /// </summary>
        /// <returns>Last item in the queue if present; null otherwise.</returns>
        protected override CrawlerQueueEntry PopImpl()
        {
            using (var model = new NCrawlerModel())
            {
                CrawlQueue result = model.CrawlQueues.FirstOrDefault(q => q.GroupId == this.groupId);
                if (result.IsNull())
                {
                    return null;
                }

                model.CrawlQueues.Remove(result);
                model.SaveChanges();
                return result.SerializedData.FromJson<CrawlerQueueEntry>();
            }
        }

        /// <summary>
        /// Push item in the queue.
        /// </summary>
        /// <param name="crawlerQueueEntry">Queue entry.</param>
        protected override void PushImpl(CrawlerQueueEntry crawlerQueueEntry)
        {
            using (var model = new NCrawlerModel())
            {
                var entry = new CrawlQueue
                {
                    GroupId = this.groupId,
                    SerializedData = crawlerQueueEntry.ToJson(),
                };

                model.CrawlQueues.Add(entry);
                model.SaveChanges();
            }
        }

        /// <summary>
        /// Cleans group queue.
        /// </summary>
        private void CleanQueue()
        {
            using (var model = new NCrawlerModel())
            {
                model.Database.ExecuteSqlCommand("DELETE FROM CrawlQueues WHERE GroupId = {0}", this.groupId);
            }
        }
    }
}
