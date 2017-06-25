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
    public class FileStorageQueueServiceTest : QueueServiceTestBase
    {
        protected override ICrawlerQueue GetCrawlQueue()
        {
            var storagePath = new FileInfo(AppContext.BaseDirectory).DirectoryName;
            return new FileCrawlQueueService(Path.Combine(storagePath, "Queue"), false);
        }
    }
}
