using System;

namespace NCrawler.Interfaces
{
	public interface IFilter
	{
		bool Match(Uri uri, CrawlStep referrer);
	}
}