using System;
using System.Text.RegularExpressions;

using NCrawler.Interfaces;

namespace NCrawler.Services
{
	public class RegexFilter : IFilter
	{
		#region Readonly & Static Fields

		private readonly Lazy<Regex> m_Regex;

		#endregion

		#region Constructors

		public RegexFilter(Regex regex)
		{
            this.m_Regex = new Lazy<Regex>(() => regex, true);
		}

		#endregion

		#region Operators

		public static explicit operator RegexFilter(Regex regex)
		{
			return new RegexFilter(regex);
		}

		#endregion

		#region IFilter Members

		public bool Match(Uri uri, CrawlStep referrer)
		{
			return this.m_Regex.Value.Match(uri.ToString()).Success;
		}

		#endregion
	}
}