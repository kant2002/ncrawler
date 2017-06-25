using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCrawler.Interfaces;
using NCrawler.Test;
using NUnit.Framework;

namespace NCrawler.EsentServices.Tests
{
    [TestFixture]
    public class EsentQueueServiceTest : QueueServiceTestBase
    {
        protected override ICrawlerQueue GetCrawlQueue()
        {
            return new EsentCrawlQueueService(
                AppContext.BaseDirectory,
                new Uri("http://www.ncrawler.com"),
                false);
        }
    }
}
