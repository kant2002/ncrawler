// -----------------------------------------------------------------------
// <copyright file="CrawlQueue.cs" company="Andrey Kurdiumov">
// Copyright (c) Andrey Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace NCrawler.EntityFramework
{
    /// <summary>
    /// Crawling entry in the database.
    /// </summary>
    public class CrawlQueue
    {
        /// <summary>
        /// Gets or sets id of the queue entry in the database.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets id of the crawling group.
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets serialized data.
        /// </summary>
        public string SerializedData { get; set; }
    }
}
