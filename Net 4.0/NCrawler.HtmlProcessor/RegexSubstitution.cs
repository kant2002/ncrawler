using System;
using System.Text.RegularExpressions;

using NCrawler.HtmlProcessor.Interfaces;

namespace NCrawler.HtmlProcessor
{
	public class RegexSubstitution : ISubstitution
	{
		#region Readonly & Static Fields

		private readonly Lazy<Regex> m_Match;
		private readonly string m_Replacement;

		#endregion

		#region Constructors

		public RegexSubstitution(Regex match, string replacement)
		{
            this.m_Match = new Lazy<Regex>(() => match, true);
            this.m_Replacement = replacement;
		}

		#endregion

		#region ISubstitution Members

		public string Substitute(string original, CrawlStep crawlStep)
		{
			return this.m_Match.Value.Replace(original, this.m_Replacement);
		}

		#endregion
	}
}