using System;
using System.Threading.Tasks;

namespace NCrawler.Interfaces
{
	public interface IRobot
	{
		Task<bool> IsAllowed(string userAgent, Uri uri);
	}
}