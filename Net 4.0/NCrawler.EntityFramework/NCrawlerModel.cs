// -----------------------------------------------------------------------
// <copyright file="NCrawlerModel.cs" company="Andrey Kurdiumov">
// Copyright (c) Andrey Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace NCrawler.EntityFramework
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    /// <summary>
    /// Database context.
    /// </summary>
    public class NCrawlerModel : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NCrawlerModel"/> class.
        /// </summary>
        public NCrawlerModel()
            : base("name=NCrawlerModel")
        {
        }

        /// <summary>
        /// Gets or sets set of crawl queue entries.
        /// </summary>
        public virtual DbSet<CrawlQueue> CrawlQueues { get; set; }

        /// <summary>
        /// Gets or sets set of crawl history entries.
        /// </summary>
        public virtual DbSet<CrawlHistory> CrawlHistories { get; set; }
    }
}