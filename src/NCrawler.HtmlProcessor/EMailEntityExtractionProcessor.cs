// --------------------------------------------------------------------------------------------------------------------- 
// <copyright file="EMailEntityExtractionProcessor.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the EMailEntityExtractionProcessor type.
// </summary>
// ---------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NCrawler.Extensions;
using NCrawler.Interfaces;
using NCrawler.Utils;

namespace NCrawler.HtmlProcessor
{
	public class EMailEntityExtractionProcessor : IPipelineStep
	{
		private const RegexOptions Options = RegexOptions.IgnoreCase |
			RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled;

		private static readonly Lazy<Regex> defaultEmailRegex = new Lazy<Regex>(() => new Regex(
			"([a-zA-Z0-9_\\-\\.]+)@((\\[[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1" +
				",3}\\.)|(([a-zA-Z0-9\\-]+\\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})",
			Options), true);

        private Lazy<Regex> emailRegex;

        /// <summary>
        /// Initializes a new intance of the <see cref="EMailEntityExtractionProcessor"/> with default email regular expression.
        /// </summary>
        public EMailEntityExtractionProcessor()
        {
            this.emailRegex = defaultEmailRegex;
        }

        /// <summary>
        /// Initializes a new intance of the <see cref="EMailEntityExtractionProcessor"/> with default email regular expression.
        /// </summary>
        /// <param name="emailMatcher">Regular expression which matches the email addresses.</param>
        public EMailEntityExtractionProcessor(Regex emailMatcher)
        {
            this.emailRegex = new Lazy<Regex>(() => emailMatcher);
        }

        /// <summary>
        /// </summary>
        /// <param name="crawler">
        /// The crawler.
        /// </param>
        /// <param name="propertyBag">
        /// The property bag.
        /// </param>
        public Task ProcessAsync(Crawler crawler, PropertyBag propertyBag)
		{
			AspectF.Define.
				NotNull(crawler, "crawler").
				NotNull(propertyBag, "propertyBag");

			var text = propertyBag.Text;
			if (string.IsNullOrEmpty(text))
			{
				return Task.CompletedTask;
			}

			var matches = this.emailRegex.Value.Matches(text);
			propertyBag["Email"].Value = matches.Cast<Match>().
				Select(match => match.Value).
				Join(";");
            return Task.CompletedTask;
        }
	}
}