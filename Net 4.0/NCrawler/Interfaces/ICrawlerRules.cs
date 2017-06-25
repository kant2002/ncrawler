using System;
using System.Threading.Tasks;

namespace NCrawler.Interfaces
{
	public interface ICrawlerRules
	{
        /// <summary>
        /// 	Checks if the crawler should follow an url
        /// </summary>
        /// <param name = "uri">Url to check</param>
        /// <param name = "referrer"></param>
        /// <returns>True if the crawler should follow the url, else false</returns>
        Task<bool> IsAllowedUrlAsync(Uri uri, CrawlStep referrer);

		bool IsExternalUrl(Uri uri);
	}
}