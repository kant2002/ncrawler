using System;
using System.Threading;

namespace NCrawler.Extensions
{
	public static class IAsyncResultExtensions
	{
		#region Class Methods

		public static void FromAsync(this IAsyncResult asyncResult, Action<IAsyncResult, bool> endMethod, TimeSpan? timeout)
		{
			int timeoutValue = -1;
			if (timeout.HasValue)
			{
				timeoutValue = (int)timeout.Value.TotalMilliseconds;
			}

			ThreadPool.RegisterWaitForSingleObject(asyncResult.AsyncWaitHandle,
				(s, isTimedout) => endMethod(asyncResult, isTimedout), null,
				timeoutValue, true);
		}

		#endregion
	}
}