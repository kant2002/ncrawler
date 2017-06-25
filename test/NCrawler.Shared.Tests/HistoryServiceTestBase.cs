using System;
using System.IO;
using System.Reflection;

using NCrawler.Interfaces;
using NCrawler.Services;
using NCrawler.Test.Helpers;

using NUnit.Framework;

namespace NCrawler.Test
{
	public abstract class HistoryServiceTestBase
	{
        protected abstract ICrawlerHistory GetCrawlerHistory();

        [Test]
        public void Test1()
		{
            var crawlerHistory = GetCrawlerHistory();
            Assert.NotNull(crawlerHistory);
			Assert.AreEqual(0, crawlerHistory.RegisteredCount);

			if (crawlerHistory is IDisposable)
			{
				((IDisposable)crawlerHistory).Dispose();
			}
		}

        [Test]
        public void Test2()
		{
            var crawlerHistory = GetCrawlerHistory();
            Assert.NotNull(crawlerHistory);
            crawlerHistory.Register("123");
			Assert.AreEqual(1, crawlerHistory.RegisteredCount);

			if (crawlerHistory is IDisposable)
			{
				((IDisposable)crawlerHistory).Dispose();
			}
		}

        [Test]
        public void Test3()
		{
            var crawlerHistory = GetCrawlerHistory();
            Assert.NotNull(crawlerHistory);
			Assert.IsTrue(crawlerHistory.Register("123"));
			Assert.IsFalse(crawlerHistory.Register("123"));

			if (crawlerHistory is IDisposable)
			{
				((IDisposable)crawlerHistory).Dispose();
			}
		}

        [Test]
        public void Test4()
		{
            var crawlerHistory = GetCrawlerHistory();
            Assert.NotNull(crawlerHistory);
			Assert.IsTrue(crawlerHistory.Register("123"));
			Assert.IsTrue(crawlerHistory.Register("1234"));

			if (crawlerHistory is IDisposable)
			{
				((IDisposable)crawlerHistory).Dispose();
			}
		}

        [Test]
        public void Test5()
		{
            var crawlerHistory = GetCrawlerHistory();
            Assert.NotNull(crawlerHistory);

			for (int i = 0; i < 10; i++)
			{
				crawlerHistory.Register(i.ToString());
			}

			for (int i = 0; i < 10; i++)
			{
				Assert.IsFalse(crawlerHistory.Register(i.ToString()));
			}

			for (int i = 10; i < 20; i++)
			{
				Assert.IsTrue(crawlerHistory.Register(i.ToString()));
			}

			if (crawlerHistory is IDisposable)
			{
				((IDisposable)crawlerHistory).Dispose();
			}
		}

        [Test]
        public void Test6()
		{
            var crawlerHistory = GetCrawlerHistory();
            Assert.NotNull(crawlerHistory);

			int count = 0;
			foreach (string url in new StringPatternGenerator("http://ncrawler[a,b,c,d,e,f].codeplex.com/view[0-10].aspx?param1=[a-c]&param2=[D-F]"))
			{
				Assert.IsTrue(crawlerHistory.Register(url));
				Assert.IsFalse(crawlerHistory.Register(url));
				count++;
				Assert.AreEqual(count, crawlerHistory.RegisteredCount);
			}

			if (crawlerHistory is IDisposable)
			{
				((IDisposable)crawlerHistory).Dispose();
			}
		}
	}
}