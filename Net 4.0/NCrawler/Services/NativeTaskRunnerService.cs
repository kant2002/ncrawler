using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

using NCrawler.Extensions;
using NCrawler.Interfaces;

namespace NCrawler.Services
{
	public class NativeTaskRunnerService : ITaskRunner
	{
		#region ITaskRunner Members

		/// <summary>
		/// 	Returns true on successfull run without timeout
		/// </summary>
		/// <param name = "action"></param>
		/// <param name = "maxRuntime"></param>
		/// <returns>True on success</returns>
		public bool RunSync(Action<CancelEventArgs> action, TimeSpan maxRuntime)
		{
			Exception exception = null;
			using (var cancelSource = new CancellationTokenSource())
			{
				var args = new CancelEventArgs(false);
				var task = Task.Factory.StartNew(() =>
					{
						try
						{
							action(args);
						}
						catch (Exception ex)
						{
							exception = ex;
						}
					}, cancelSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
				var success = task.Wait(maxRuntime);
				if (!success)
				{
					cancelSource.Cancel();
					return false;
				}
			}

			if (!exception.IsNull())
			{
				throw exception;
			}

			return true;
		}

		#endregion
	}
}