using System;
using System.Threading;

using NCrawler.Extensions;
using NCrawler.Interfaces;

namespace NCrawler.Utils
{
	public abstract class HistoryServiceBase : DisposableBase, ICrawlerHistory
	{
		#region Readonly & Static Fields

		private readonly ReaderWriterLockSlim m_CrawlHistoryLock =
			new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

		#endregion

		#region Instance Methods

		protected abstract void Add(string key);
		protected abstract bool Exists(string key);
		protected abstract long GetRegisteredCount();

		protected override void Cleanup()
		{
            this.m_CrawlHistoryLock.Dispose();
		}

		#endregion

		#region ICrawlerHistory Members

		public virtual long RegisteredCount
		{
			get
			{
				return AspectF.Define.
					ReadLock(this.m_CrawlHistoryLock).
					Return(() => this.GetRegisteredCount());
			}
		}

		public virtual bool Register(string key)
		{
            if (key.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(key));
            }

            return AspectF.Define.
				ReadLockUpgradable(this.m_CrawlHistoryLock).
				Return(() =>
					{
						var exists = this.Exists(key);
						if (!exists)
						{
							AspectF.Define.
								WriteLock(this.m_CrawlHistoryLock).
								Do(() => this.Add(key));
						}

						return !exists;
					});
		}

		#endregion
	}
}