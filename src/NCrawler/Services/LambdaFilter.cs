using System;

using NCrawler.Extensions;
using NCrawler.Interfaces;

namespace NCrawler.Services
{
	public class LambdaFilter : IFilter
	{
		#region Readonly & Static Fields

		private readonly Func<Uri, CrawlStep, bool> m_Match;
		private readonly Func<Uri, bool> m_Match2;

		#endregion

		#region Constructors

		public LambdaFilter(Func<Uri, CrawlStep, bool> match)
		{
            this.m_Match = match;
		}

		public LambdaFilter(Func<Uri, bool> match2)
		{
            this.m_Match2 = match2;
		}

		#endregion

		#region Operators

		public static explicit operator LambdaFilter(Func<Uri, bool> match)
		{
			return new LambdaFilter(match);
		}

		public static explicit operator LambdaFilter(Func<Uri, CrawlStep, bool> match)
		{
			return new LambdaFilter(match);
		}

		#endregion

		#region IFilter Members

		public bool Match(Uri uri, CrawlStep referrer)
		{
			if (!this.m_Match.IsNull())
			{
				return this.m_Match(uri, referrer);
			}

			if (!this.m_Match2.IsNull())
			{
				return this.m_Match2(uri);
			}

			return false;
		}

		#endregion
	}
}