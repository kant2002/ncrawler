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
    public class EsentHistoryServiceTest : HistoryServiceTestBase
    {
        protected override ICrawlerHistory GetCrawlerHistory()
        {
            return new EsentCrawlerHistoryService(
                AppContext.BaseDirectory,
                new Uri("http://www.ncrawler.com"),
                false);
        }
    }
}
