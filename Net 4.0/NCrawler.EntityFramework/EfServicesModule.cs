// -----------------------------------------------------------------------
// <copyright file="EfServicesModule.cs" company="Andrey Kurdiumov">
// Copyright (c) Andrey Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace NCrawler.EntityFramework
{
    using System;

    using Autofac;

    using NCrawler.Interfaces;

    /// <summary>
    /// Module which provides EF backed history and crawl queue.
    /// </summary>
    public class EfServicesModule : NCrawlerModule
    {
        /// <summary>
        /// Flag indicating whether module should resume previous work or not.
        /// </summary>
        private readonly bool resume;

        /// <summary>
        /// Initializes a new instance of the <see cref="EfServicesModule"/> class.
        /// </summary>
        /// <param name="resume">True if module resume his work; false otherwise.</param>
        public EfServicesModule(bool resume)
        {
            this.resume = resume;
        }

        /// <summary>
        /// Setup module as main module.
        /// </summary>
        /// <param name="resume">True if module resume his work; false otherwise.</param>
        public static void Setup(bool resume)
        {
            NCrawlerModule.Setup(new EfServicesModule(resume));
        }

        /// <summary>
        /// Performs load of the module to the IoC.
        /// </summary>
        /// <param name="builder">Container builder where to register.</param>
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.Register((c, p) => new EfCrawlerHistoryService(p.TypedAs<Uri>(), this.resume))
                .As<ICrawlerHistory>().InstancePerDependency();
            builder.Register((c, p) => new EfCrawlQueueService(p.TypedAs<Uri>(), this.resume))
                .As<ICrawlerQueue>().InstancePerDependency();
        }
    }
}
