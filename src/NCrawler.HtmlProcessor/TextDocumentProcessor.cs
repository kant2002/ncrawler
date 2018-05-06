using System;
using System.Net;
using System.Threading.Tasks;
using NCrawler.Extensions;
using NCrawler.Interfaces;

namespace NCrawler.HtmlProcessor
{
	public class TextDocumentProcessor : IPipelineStep
	{
		#region IPipelineStep Members

		public Task ProcessAsync(ICrawler crawler, PropertyBag propertyBag)
		{
            return this.ProcessCoreAsync(crawler, propertyBag);
        }

        #endregion

        #region Class Methods

        private async Task<int> ProcessCoreAsync(ICrawler crawler, PropertyBag propertyBag)
        {
            if (propertyBag.StatusCode != HttpStatusCode.OK)
            {
                return 0;
            }

            if (!IsTextContent(propertyBag.ContentType))
            {
                return 0;
            }

            using (var reader = propertyBag.GetResponse())
            {
                var content = await reader.ReadToEndAsync();
                propertyBag.Text = content.Trim();
            }

            return 0;
        }

        private static bool IsTextContent(string contentType)
		{
			return contentType.StartsWith("text/plain", StringComparison.OrdinalIgnoreCase);
		}

		#endregion
	}
}