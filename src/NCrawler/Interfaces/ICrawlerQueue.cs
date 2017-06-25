namespace NCrawler.Interfaces
{
	public interface ICrawlerQueue
	{
		long Count { get; }

		/// <summary>
		/// 	Get next entry to crawl
		/// </summary>
		/// <returns></returns>
		CrawlerQueueEntry Pop();

		/// <summary>
		/// 	Queue entry to crawl
		/// </summary>
		void Push(CrawlerQueueEntry crawlerQueueEntry);
	}
}