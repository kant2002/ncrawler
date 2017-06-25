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
    public class IsolatedStorageHistoryServiceTest : HistoryServiceTestBase
    {
        protected override ICrawlerHistory GetCrawlerHistory()
        {
            return new IsolatedStorageCrawlerHistoryService(new Uri("http://www.ncrawler.com"), false);
        }
    }
}
