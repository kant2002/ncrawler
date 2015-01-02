// -----------------------------------------------------------------------
// <copyright file="CrawlHistory.cs" company="Andrey Kurdiumov">
// Copyright (c) Andrey Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace NCrawler.EntityFramework
{
    /// <summary>
    /// Crawling history item.
    /// </summary>
    public class CrawlHistory
    {
        /// <summary>
        /// Gets or sets id of the history entry in the database.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets key if the item in the history.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets id of the crawling group.
        /// </summary>
        public int GroupId { get; set; }
    }
}
