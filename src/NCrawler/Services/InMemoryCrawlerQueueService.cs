using System.Collections.Generic;

using NCrawler.Utils;

namespace NCrawler.Services
{
	public class InMemoryCrawlerQueueService : CrawlerQueueServiceBase
	{
		#region Readonly & Static Fields

		private readonly Stack<CrawlerQueueEntry> m_Stack = new Stack<CrawlerQueueEntry>();

		#endregion

		#region Instance Methods

		protected override long GetCount()
		{
			return this.m_Stack.Count;
		}

		protected override CrawlerQueueEntry PopImpl()
		{
			return this.m_Stack.Count == 0 ? null : this.m_Stack.Pop();
		}

		protected override void PushImpl(CrawlerQueueEntry crawlerQueueEntry)
		{
            this.m_Stack.Push(crawlerQueueEntry);
		}

		#endregion
	}
}