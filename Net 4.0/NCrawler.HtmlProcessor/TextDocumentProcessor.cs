using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using NCrawler.Extensions;
using NCrawler.Interfaces;

namespace NCrawler.HtmlProcessor
{
	public class TextDocumentProcessor : IPipelineStep
	{
		#region IPipelineStep Members

		public Task ProcessAsync(Crawler crawler, PropertyBag propertyBag)
		{
			if (propertyBag.StatusCode != HttpStatusCode.OK)
			{
				return Task.FromResult(0);
			}

			if (!IsTextContent(propertyBag.ContentType))
			{
				return Task.FromResult(0);
			}

			using (var reader = propertyBag.GetResponse())
			{
				var content = reader.ReadToEnd();
				propertyBag.Text = content.Trim();
            }

            return Task.FromResult(0);
        }

		#endregion

		#region Class Methods

		private static bool IsTextContent(string contentType)
		{
			return contentType.StartsWith("text/plain", StringComparison.OrdinalIgnoreCase);
		}

		#endregion
	}
}