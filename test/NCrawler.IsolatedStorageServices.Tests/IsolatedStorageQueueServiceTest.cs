using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCrawler.Interfaces;
using NCrawler.Test;
using NUnit.Framework;

namespace NCrawler.IsolatedStorageServices.Tests
{
    [TestFixture]
    public class IsolatedStorageQueueServiceTest : QueueServiceTestBase
    {
        protected override ICrawlerQueue GetCrawlQueue()
        {
            return new IsolatedStorageCrawlerQueueService(new Uri("http://www.ncrawler.com"), false);
        }
    }
}
