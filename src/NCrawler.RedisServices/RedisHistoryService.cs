using System;

using NCrawler.Utils;

using ServiceStack.Redis;
using ServiceStack.Redis.Generic;

namespace NCrawler.RedisServices
{
	public class RedisHistoryService : HistoryServiceBase
	{
		#region Readonly & Static Fields

		private readonly IRedisSet<string> _history;
		private readonly IRedisTypedClient<string> _redis;

		#endregion

		#region Constructors

		public RedisHistoryService(Uri baseUri, bool resume)
		{
			using (var redisClient = new RedisClient())
			{
                this._redis = redisClient.As<string>();
                this._history = this._redis.Sets[string.Format("barcodes:{0}:history", baseUri)];
				if (!resume)
				{
                    this._history.Clear();
				}
			}
		}

		#endregion

		#region Instance Methods

		protected override void Add(string key)
		{
            this._history.Add(key);
		}

		protected override bool Exists(string key)
		{
			return this._history.Contains(key);
		}

		protected override long GetRegisteredCount()
		{
			return this._history.Count;
		}

		#endregion
	}
}