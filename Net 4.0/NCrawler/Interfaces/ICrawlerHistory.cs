namespace NCrawler.Interfaces
{
	public interface ICrawlerHistory
	{
		long RegisteredCount { get; }

		/// <summary>
		/// 	Register a unique key
		/// </summary>
		/// <param name = "key">key to register</param>
		/// <returns>false if key has already been registered else true</returns>
		bool Register(string key);
	}
}