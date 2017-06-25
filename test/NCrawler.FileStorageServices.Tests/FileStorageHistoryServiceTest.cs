using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NCrawler.Interfaces;
using NCrawler.Test;
using NUnit.Framework;

namespace NCrawler.FileStorageServices.Tests
{
    [TestFixture]
    public class FileStorageHistoryServiceTest : HistoryServiceTestBase
    {
        protected override ICrawlerHistory GetCrawlerHistory()
        {
            var storagePath = new FileInfo(AppContext.BaseDirectory).DirectoryName;
            return new FileCrawlHistoryService(Path.Combine(storagePath, "History"), false);
        }
    }
}
