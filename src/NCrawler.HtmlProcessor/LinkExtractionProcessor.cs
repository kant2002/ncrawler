using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NCrawler.Extensions;
using NCrawler.HtmlProcessor.Properties;
using NCrawler.Interfaces;

namespace NCrawler.HtmlProcessor
{
	public class LinkExtractionProcessor : ContentCrawlerRules, IPipelineStep
	{
		#region Readonly & Static Fields

		private static readonly Lazy<Regex> s_LinkRegex = new Lazy<Regex>(() => new Regex(
			"(?<Protocol>\\w+):\\/\\/(?<Domain>[\\w@][\\w.:@]+)\\/?[\\w\\.?=%&=\\-@/$,]*",
			RegexOptions.IgnoreCase | RegexOptions.CultureInvariant |
				RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled), true);

		#endregion

		#region Constructors

		public LinkExtractionProcessor()
		{
		}

		public LinkExtractionProcessor(Dictionary<string, string> filterTextRules, Dictionary<string, string> filterLinksRules)
			: base(filterTextRules, filterLinksRules)
		{
		}

        #endregion

        #region IPipelineStep Members

        public virtual async Task ProcessAsync(ICrawler crawler, PropertyBag propertyBag)
        {
            // Get text from previous pipeline step
            var text = propertyBag.Text;
            if (this.HasTextStripRules)
            {
                text = StripText(text);
            }

            if (text.IsNullOrEmpty())
            {
                return;
            }

            if (this.HasLinkStripRules)
            {
                text = StripLinks(text);
            }

            // Find links
            var matches = s_LinkRegex.Value.Matches(text);
            foreach (var match in matches.Cast<Match>().Where(m => m.Success))
            {
                var link = match.Value;
                if (link.IsNullOrEmpty())
                {
                    continue;
                }

                var baseUrl = propertyBag.ResponseUri.GetLeftPath();
                var normalizedLink = link.NormalizeUrl(baseUrl);
                if (normalizedLink.IsNullOrEmpty())
                {
                    continue;
                }

                // Add new step to crawler
                await crawler.AddStepAsync(new Uri(normalizedLink), propertyBag.Step.Depth + 1,
                    propertyBag.Step, new Dictionary<string, object>
                        {
                            {Resources.PropertyBagKeyOriginalUrl, new Uri(link)},
                            {Resources.PropertyBagKeyOriginalReferrerUrl, propertyBag.ResponseUri}
                        });
            }
        }

        #endregion
    }
}