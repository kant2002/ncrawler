using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using NCrawler.Interfaces;
using NCrawler.Utils;

namespace NCrawler.Extensions
{
	public static class AspectExtensions
	{
        #region Class Methods

        [DebuggerStepThrough]
		public static AspectF Cache<TReturnType>(this AspectF aspect,
			ICache cacheResolver, string key)
		{
			return aspect.Combine(work => Cache<TReturnType>(aspect, cacheResolver, key, work, cached => cached));
		}

		[DebuggerStepThrough]
		public static AspectF IgnoreException<T>(this AspectF aspect) where T : Exception
		{
			return aspect.Combine(work =>
				{
					try
					{
						work();
					}
					catch (T)
					{
					}
				});
		}

		[DebuggerStepThrough]
		public static AspectF NotNull(this AspectF aspect, object @object, string parameterName)
		{
			if (@object.IsNull())
			{
				throw new ArgumentNullException(parameterName);
			}

			return aspect;
		}

		[DebuggerStepThrough]
		public static AspectF ReadLock(this AspectF aspect, ReaderWriterLockSlim @lock)
		{
			return aspect.Combine(work =>
				{
					@lock.EnterReadLock();
					try
					{
						work();
					}
					finally
					{
						@lock.ExitReadLock();
					}
				});
		}

		[DebuggerStepThrough]
		public static AspectF ReadLockUpgradable(this AspectF aspect, ReaderWriterLockSlim @lock)
		{
			return aspect.Combine(work =>
				{
					@lock.EnterUpgradeableReadLock();
					try
					{
						work();
					}
					finally
					{
						@lock.ExitUpgradeableReadLock();
					}
				});
		}

		[DebuggerStepThrough]
		public static AspectF RunAsync(this AspectF aspect, Action completeCallback)
		{
			return aspect.Combine(work => work.BeginInvoke(asyncresult =>
				{
					work.EndInvoke(asyncresult);
					completeCallback();
				}, null));
		}

		[DebuggerStepThrough]
		public static AspectF Until(this AspectF aspect, Func<bool> test)
		{
			return aspect.Combine(work =>
				{
					while (!test()) ;
					work();
				});
		}

		[DebuggerStepThrough]
		public static AspectF WhenTrue(this AspectF aspect, params Func<bool>[] conditions)
		{
			return aspect.Combine(work =>
				{
					if (conditions.Any(condition => !condition()))
					{
						return;
					}

					work();
				});
		}

		[DebuggerStepThrough]
		public static AspectF While(this AspectF aspect, Func<bool> test)
		{
			return aspect.Combine(work =>
				{
					while (test())
					{
						work();
					}
				});
		}

		[DebuggerStepThrough]
		public static AspectF WriteLock(this AspectF aspect, ReaderWriterLockSlim @lock)
		{
			return aspect.Combine(work =>
				{
					@lock.EnterWriteLock();
					try
					{
						work();
					}
					finally
					{
						@lock.ExitWriteLock();
					}
				});
		}

		private static void Cache<TReturnType>(AspectF aspect, ICache cacheResolver,
			string key, Action work, Func<TReturnType, TReturnType> foundInCache)
		{
			var cachedData = cacheResolver.Get(key);
			if (cachedData.IsNull())
			{
				GetListFromSource<TReturnType>(aspect, cacheResolver, key);
			}
			else
			{
				// Give caller a chance to shape the cached item before it is returned
				var cachedType = foundInCache((TReturnType) cachedData);
				if (cachedType.IsNull())
				{
					GetListFromSource<TReturnType>(aspect, cacheResolver, key);
				}
				else
				{
					aspect.m_WorkDelegate = new Func<TReturnType>(() => cachedType);
				}
			}

			work();
		}

		private static void GetListFromSource<TReturnType>(AspectF aspect, ICache cacheResolver, string key)
		{
			Func<TReturnType> workDelegate = (Func<TReturnType>) aspect.m_WorkDelegate;
			var realObject = workDelegate();
			cacheResolver.Add(key, realObject);
			workDelegate = () => realObject;
			aspect.m_WorkDelegate = workDelegate;
		}

#endregion
	}
}