using System.Collections.Generic;
using System.Linq;

using HtmlAgilityPack;

using NCrawler.Extensions;
using NCrawler.Utils;

namespace NCrawler.HtmlProcessor.Extensions
{
    public class DocumentWithLinks
	{
		private readonly HtmlDocument m_Doc;

		/// <summary>
		/// Creates an instance of a DocumentWithLinkedFiles.
		/// </summary>
		/// <param name="doc">The input HTML document. May not be null.</param>
		public DocumentWithLinks(HtmlDocument doc)
		{
			AspectF.Define.
				NotNull(doc, "doc");

            this.m_Doc = doc;
            this.GetLinks();
            this.GetReferences();
		}

		/// <summary>
		/// Gets a list of links as they are declared in the HTML document.
		/// </summary>
		public IEnumerable<string> Links { get; private set; }

		/// <summary>
		/// Gets a list of reference links to other HTML documents, as they are declared in the HTML document.
		/// </summary>
		public IEnumerable<string> References { get; private set; }

		private void GetLinks()
		{
			var atts = this.m_Doc.DocumentNode.SelectNodes("//*[@background or @lowsrc or @src or @href or @action]");
			if (atts.IsNull())
			{
                this.Links = System.Array.Empty<string>();
				return;
			}

            this.Links = atts.
				SelectMany(n => new[]
					{
						ParseLink(n, "background"),
						ParseLink(n, "href"),
						ParseLink(n, "src"),
						ParseLink(n, "lowsrc"),
						ParseLink(n, "action"),
					}).
				Distinct().
				ToArray();
		}

		private void GetReferences()
		{
			var hrefs = this.m_Doc.DocumentNode.SelectNodes("//a[@href]");
			if (hrefs.IsNull())
			{
                this.References = System.Array.Empty<string>();
				return;
			}

            this.References = hrefs.
				Select(href => href.Attributes["href"].Value).
				Distinct().
				ToArray();
		}

		private static string ParseLink(HtmlNode node, string name)
		{
			var att = node.Attributes[name];
			if (att.IsNull())
			{
				return null;
			}

			// if name = href, we are only interested by <link> tags
			if ((name == "href") && (node.Name != "link"))
			{
				return null;
			}

			return att.Value;
		}
	}
}