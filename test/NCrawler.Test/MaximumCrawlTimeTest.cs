using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NCrawler.HtmlProcessor;
using NCrawler.Test.Helpers;

using NUnit.Framework;

namespace NCrawler.Test
{
	[TestFixture]
	public class MaximumCrawlTimeTest
	{
		[Test]
		public async Task MaximumCrawlTime()
		{
			TestModule.SetupInMemoryStorage();

			// Setup
			Stopwatch timer;
			using (var c = new Crawler(new Uri("http://ncrawler.codeplex.com"), new HtmlDocumentProcessor())
				{
					// Custom step to visualize crawl
					MaximumThreadCount = 10,
					MaximumCrawlDepth = 10,
					MaximumCrawlTime = TimeSpan.FromSeconds(2)
				})
			{
				timer = Stopwatch.StartNew();

				// Run
				await c.CrawlAsync().ConfigureAwait(false);
				timer.Stop();
			}

			// Allow time for gracefull finish
			Assert.Less(timer.ElapsedMilliseconds, 10000);
		}
	}
}