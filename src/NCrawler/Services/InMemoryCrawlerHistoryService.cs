using System.Collections.Generic;

using NCrawler.Utils;

namespace NCrawler.Services
{
	public class InMemoryCrawlerHistoryService : HistoryServiceBase
	{
		#region Readonly & Static Fields

		private readonly HashSet<string> m_VisitedUrls = new HashSet<string>();

		#endregion

		#region Instance Methods

		protected override void Add(string key)
		{
            this.m_VisitedUrls.Add(key);
		}

		protected override bool Exists(string key)
		{
			return this.m_VisitedUrls.Contains(key);
		}

		protected override long GetRegisteredCount()
		{
			return this.m_VisitedUrls.Count;
		}

		#endregion
	}
}