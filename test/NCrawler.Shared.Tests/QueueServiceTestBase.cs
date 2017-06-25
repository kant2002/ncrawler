using System;

using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using NCrawler.HtmlProcessor;
using NCrawler.Interfaces;
using NCrawler.Services;
using NCrawler.Test.Helpers;

using NUnit.Framework;
using System.Threading.Tasks;

namespace NCrawler.Test
{
	public abstract class QueueServiceTestBase
	{
        protected abstract ICrawlerQueue GetCrawlQueue();

        [Test]
        public void QueueCreatedEmpty()
		{
            var crawlQueue = this.GetCrawlQueue();
            Assert.NotNull(crawlQueue);
			Assert.AreEqual(0, crawlQueue.Count);

			if(crawlQueue is IDisposable)
			{
				((IDisposable)crawlQueue).Dispose();
			}
		}

        [Test]
        public void AfterAddingElementSingleItemInQueue()
		{
            var crawlQueue = this.GetCrawlQueue();
            Assert.NotNull(crawlQueue);
			crawlQueue.Push(new CrawlerQueueEntry());
			Assert.AreEqual(1, crawlQueue.Count);

			if (crawlQueue is IDisposable)
			{
				((IDisposable)crawlQueue).Dispose();
			}
		}

        [Test]
        public void AfterPopElementQueueIsEmpty()
		{
            var crawlQueue = this.GetCrawlQueue();
            Assert.NotNull(crawlQueue);
			crawlQueue.Push(new CrawlerQueueEntry());
			crawlQueue.Pop();
			Assert.AreEqual(0, crawlQueue.Count);

			if (crawlQueue is IDisposable)
			{
				((IDisposable)crawlQueue).Dispose();
			}
		}

		public void CouldNotPopFromEmptyQueue()
		{
            var crawlQueue = this.GetCrawlQueue();
            Assert.NotNull(crawlQueue);
			crawlQueue.Push(new CrawlerQueueEntry());
			crawlQueue.Pop();
			Assert.AreEqual(0, crawlQueue.Count);
			var actualValue = crawlQueue.Pop();
			Assert.IsNull(actualValue);

			if (crawlQueue is IDisposable)
			{
				((IDisposable)crawlQueue).Dispose();
			}
		}

        [Test]
        public void PropertiesPersistedAfterPop()
		{
            var crawlQueue = this.GetCrawlQueue();
            Assert.NotNull(crawlQueue);
			var now = DateTime.Now;
			crawlQueue.Push(new CrawlerQueueEntry
				{
					CrawlStep = new CrawlStep(new Uri("http://www.biz.org/"), 0),
					Properties = new Dictionary<string, object>
						{
							{"one", "string"},
							{"two", 123},
							{"three", now},
						},
					Referrer = new CrawlStep(new Uri("http://www.biz3.org/"), 1)
				});
			Assert.AreEqual(1, crawlQueue.Count);
			var entry = crawlQueue.Pop();
			Assert.AreEqual(0, crawlQueue.Count);
			Assert.NotNull(entry);
			Assert.NotNull(entry.CrawlStep);
			Assert.NotNull(entry.Properties);
			Assert.NotNull(entry.Referrer);
			Assert.AreEqual(0, entry.CrawlStep.Depth);
			Assert.AreEqual("http://www.biz.org/", entry.CrawlStep.Uri.ToString());
			Assert.AreEqual("one", entry.Properties.Keys.First());
			Assert.AreEqual("two", entry.Properties.Keys.Skip(1).First());
			Assert.AreEqual("three", entry.Properties.Keys.Skip(2).First());
			Assert.AreEqual("string", entry.Properties["one"]);
			Assert.AreEqual(123, entry.Properties["two"]);
			Assert.AreEqual(now, entry.Properties["three"]);
			Assert.AreEqual(0, crawlQueue.Count);

			if (crawlQueue is IDisposable)
			{
				((IDisposable)crawlQueue).Dispose();
			}
		}

		/*[Test]
		public void TestQueuesYieldSameResult()
		{
			TestModule.SetupInMemoryStorage();
			CollectorStep reference = CollectionCrawl();
			CollectorStep inMemoryCrawlerCollectorStep = CollectionCrawl();
			Assert.AreEqual(reference.Steps.Count, inMemoryCrawlerCollectorStep.Steps.Count);

			TestModule.SetupFileStorage();
			CollectorStep fileStorageCollectorStep = CollectionCrawl();
			Assert.AreEqual(reference.Steps.Count, fileStorageCollectorStep.Steps.Count);

			TestModule.SetupIsolatedStorage();
			CollectorStep isolatedStorageServicesCollectorStep = CollectionCrawl();
			Assert.AreEqual(reference.Steps.Count, isolatedStorageServicesCollectorStep.Steps.Count);

			TestModule.SetupEfServicesStorage();
			CollectorStep dbServicesCollectorStep = CollectionCrawl();
			Assert.AreEqual(reference.Steps.Count, dbServicesCollectorStep.Steps.Count);

			TestModule.SetupESentServicesStorage();
			CollectorStep esentServicesCollectorStep = CollectionCrawl();
			Assert.AreEqual(reference.Steps.Count, esentServicesCollectorStep.Steps.Count);
        }*/

        private static CollectorStep CollectionCrawl()
        {
            var collectorStep = new CollectorStep();
            var htmlDocumentProcessor = new HtmlDocumentProcessor();
            using (var crawler = new Crawler(new Uri("http://ncrawler.codeplex.com"), collectorStep, htmlDocumentProcessor))
            {
                Console.Out.WriteLine(crawler.GetType());
                crawler.MaximumThreadCount = 5;
                crawler.UriSensitivity = UriComponents.HttpRequestUrl;
                crawler.ExcludeFilter = new[]
                    {
                        new RegexFilter(
                            new Regex(@"(\.jpg|\.css|\.js|\.gif|\.jpeg|\.png)",
                                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase))
                    };
                crawler.CrawlAsync().Wait();
                return collectorStep;
            }
        }
    }

	internal class CollectorStep : IPipelineStep
	{
		private readonly List<CrawlStep> m_Steps = new List<CrawlStep>();

		public List<CrawlStep> Steps
		{
			get{ return this.m_Steps; }
		}

		public Task ProcessAsync(Crawler crawler, PropertyBag propertyBag)
		{
            this.Steps.Add(propertyBag.Step);
            return Task.CompletedTask;
		}
	}
}