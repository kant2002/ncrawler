using System;
using System.IO;
using System.Reflection;

using NCrawler.Interfaces;
using NCrawler.Services;

using NUnit.Framework;

namespace NCrawler.Test
{
	[TestFixture]
	public class HistoryServiceTest: HistoryServiceTestBase
    {
        protected override ICrawlerHistory GetCrawlerHistory()
        {
            return new InMemoryCrawlerHistoryService();
        }
	}
}