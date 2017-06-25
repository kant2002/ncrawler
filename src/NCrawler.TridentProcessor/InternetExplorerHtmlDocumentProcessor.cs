using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using NCrawler.Extensions;
using NCrawler.HtmlProcessor;
using NCrawler.Interfaces;
using NCrawler.Utils;

namespace NCrawler.IEProcessor
{
	public class InternetExplorerHtmlDocumentProcessor : HtmlDocumentProcessor, IPipelineStepWithTimeout
	{
		#region IPipelineStepWithTimeout Members

		public TimeSpan ProcessorTimeout
		{
			get { return TimeSpan.FromSeconds(30); }
		}

		public override async Task ProcessAsync(Crawler crawler, PropertyBag propertyBag)
		{
			AspectF.Define.
				NotNull(crawler, "crawler").
				NotNull(propertyBag, "propertyBag");

			if (propertyBag.StatusCode != HttpStatusCode.OK)
			{
				return;
			}

			if (!IsHtmlContent(propertyBag.ContentType))
			{
				return;
			}

			var documentDomHtml = string.Empty;
			var tempThread = new Thread(o =>
				{
					using (var internetExplorer = new TridentBrowserForm(propertyBag.ResponseUri.ToString()))
					{
						Application.Run(internetExplorer);
						documentDomHtml = internetExplorer.DocumentDomHtml;
					}
				});
			tempThread.SetApartmentState(ApartmentState.STA);
			tempThread.Start();
			tempThread.Join();

			propertyBag.GetResponse = () => new MemoryStream(Encoding.UTF8.GetBytes(documentDomHtml));
            await base.ProcessAsync(crawler, propertyBag);
		}

		#endregion

		#region Class Methods

		private static bool IsHtmlContent(string contentType)
		{
			return contentType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase);
		}

		#endregion
	}
}