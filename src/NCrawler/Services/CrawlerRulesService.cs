using System;
using System.Linq;
using System.Threading.Tasks;
using NCrawler.Extensions;
using NCrawler.Interfaces;
using NCrawler.Utils;

namespace NCrawler.Services
{
	/// <summary>
	/// Handles logic of how to follow links when crawling
	/// </summary>
	public class CrawlerRulesService : ICrawlerRules
	{
		#region Readonly & Static Fields

		protected readonly Uri m_BaseUri;
		protected readonly Crawler m_Crawler;
		protected readonly IRobot m_Robot;

		#endregion

		#region Constructors

		public CrawlerRulesService(Crawler crawler, IRobot robot, Uri baseUri)
		{
			AspectF.Define.
				NotNull(crawler, "crawler").
				NotNull(robot, "robot").
				NotNull(baseUri, "baseUri");

            this.m_Crawler = crawler;
            this.m_Robot = robot;
            this.m_BaseUri = baseUri;
		}

        #endregion

        #region ICrawlerRules Members

        /// <summary>
        /// 	Checks if the crawler should follow an url
        /// </summary>
        /// <param name = "uri">Url to check</param>
        /// <param name = "referrer"></param>
        /// <returns>True if the crawler should follow the url, else false</returns>
        public virtual async Task<bool> IsAllowedUrlAsync(Uri uri, CrawlStep referrer)
        {
            if (this.m_Crawler.MaximumUrlSize.HasValue && this.m_Crawler.MaximumUrlSize.Value > 10 &&
                uri.ToString().Length > this.m_Crawler.MaximumUrlSize.Value)
            {
                return false;
            }

            if (!this.m_Crawler.IncludeFilter.IsNull() && this.m_Crawler.IncludeFilter.Any(f => f.Match(uri, referrer)))
            {
                return true;
            }

            if (!this.m_Crawler.ExcludeFilter.IsNull() && this.m_Crawler.ExcludeFilter.Any(f => f.Match(uri, referrer)))
            {
                return false;
            }

            if (this.IsExternalUrl(uri))
            {
                return false;
            }

            return !this.m_Crawler.AdhereToRobotRules || await this.m_Robot.IsAllowed(this.m_Crawler.UserAgent, uri).ConfigureAwait(false);
        }

        public virtual bool IsExternalUrl(Uri uri)
		{
			return this.m_BaseUri.IsHostMatch(uri);
		}

		#endregion
	}
}