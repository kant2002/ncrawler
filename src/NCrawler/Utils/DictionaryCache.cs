using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using NCrawler.Extensions;
using NCrawler.Interfaces;

namespace NCrawler.Utils
{
	public class DictionaryCache : DisposableBase, ICache
	{
		#region Readonly & Static Fields

		private readonly Dictionary<string, object> m_Cache = new Dictionary<string, object>();

		private readonly ReaderWriterLockSlim m_CacheLock =
			new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

		private readonly int m_MaxEntries;

		#endregion

		#region Constructors

		public DictionaryCache(int maxEntries)
		{
            this.m_MaxEntries = maxEntries;
		}

		#endregion

		#region Instance Methods

		protected override void Cleanup()
		{
            this.m_CacheLock.Dispose();
		}

		#endregion

		#region ICache Members

		public void Add(string key, object value)
		{
			AspectF.Define.
				WriteLock(this.m_CacheLock).
				Do(() =>
					{
						if (!this.m_Cache.ContainsKey(key))
						{
                            this.m_Cache.Add(key, value);
						}

						while (this.m_Cache.Count > this.m_MaxEntries)
						{
                            this.m_Cache.Remove(this.m_Cache.Keys.First());
						}
					});
		}

		public void Add(string key, object value, TimeSpan timeout)
		{
			Add(key, value);
		}

		public void Set(string key, object value)
		{
			AspectF.Define.
				WriteLock(this.m_CacheLock).
				Do(() => this.m_Cache[key] = value);
		}

		public void Set(string key, object value, TimeSpan timeout)
		{
			Set(key, value);
		}

		public bool Contains(string key)
		{
			return AspectF.Define.
				ReadLock(this.m_CacheLock).
				Return(() => this.m_Cache.ContainsKey(key));
		}

		public void Flush()
		{
			AspectF.Define.
				WriteLock(this.m_CacheLock).
				Do(() => this.m_Cache.Clear());
		}

		public object Get(string key)
		{
			return AspectF.Define.
				ReadLock(this.m_CacheLock).
				Return(() => this.m_Cache.ContainsKey(key) ? this.m_Cache[key] : null);
		}

		public void Remove(string key)
		{
			AspectF.Define.
				WriteLock(this.m_CacheLock).
				Do(() => this.m_Cache.Remove(key));
		}

		#endregion
	}
}