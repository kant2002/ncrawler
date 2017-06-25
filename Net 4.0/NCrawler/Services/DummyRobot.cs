using System;
using System.Threading.Tasks;
using NCrawler.Interfaces;

namespace NCrawler.Services
{
	public class DummyRobot : IRobot
	{
		#region IRobot Members

		public Task<bool> IsAllowed(string userAgent, Uri uri)
		{
			return Task.FromResult(true);
		}

		#endregion
	}
}