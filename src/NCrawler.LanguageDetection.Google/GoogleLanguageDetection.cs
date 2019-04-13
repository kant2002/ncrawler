using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Autofac;

using NCrawler.Extensions;
using NCrawler.Interfaces;
using NCrawler.Services;
using NCrawler.Utils;

namespace NCrawler.LanguageDetection.Google
{
    public class GoogleLanguageDetection : IPipelineStepWithTimeout
	{
		#region Constants

		private const int MaxPostSize = 900;

		#endregion

		#region Readonly & Static Fields

		private readonly ILog m_Logger;

		#endregion

		#region Constructors

		public GoogleLanguageDetection()
		{
            this.m_Logger = NCrawlerModule.Container.Resolve<ILog>();
		}

		#endregion

		#region IPipelineStepWithTimeout Members

		public async Task ProcessAsync(ICrawler crawler, PropertyBag propertyBag)
		{
			AspectF.Define.
				NotNull(crawler, "crawler").
				NotNull(propertyBag, "propertyBag");

			var content = propertyBag.Text;
			if (content.IsNullOrEmpty())
			{
				return;
			}

			var contentLookupText = content.Max(MaxPostSize);
			var encodedRequestUrlFragment =
				"http://ajax.googleapis.com/ajax/services/language/detect?v=1.0&q={0}".FormatWith(contentLookupText);

            this.m_Logger.Verbose("Google language detection using: {0}", encodedRequestUrlFragment);

			try
			{
				var downloader = NCrawlerModule.Container.Resolve<IWebDownloader>();
				var result = await downloader.DownloadAsync(new CrawlStep(new Uri(encodedRequestUrlFragment), 0), null, DownloadMethod.GET).ConfigureAwait(false);
				if (result.IsNull())
				{
					return;
				}

				using (var responseReader = result.GetResponse())
				using (var reader = new StreamReader(responseReader))
				{
					var json = await reader.ReadLineAsync().ConfigureAwait(false);
					using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
					{
						var ser =
							new DataContractJsonSerializer(typeof (LanguageDetector));
						var detector = ser.ReadObject(ms) as LanguageDetector;

						if (!detector.IsNull())
						{
							var culture = CultureInfo.GetCultureInfo(detector.responseData.language);
							propertyBag["Language"].Value = detector.responseData.language;
							propertyBag["LanguageCulture"].Value = culture;
						}
					}
				}
			}
			catch (Exception ex)
			{
                this.m_Logger.Error("Error during google language detection, the error was: {0}", ex.ToString());
			}
		}

		public TimeSpan ProcessorTimeout
		{
			get { return TimeSpan.FromSeconds(10); }
		}

		#endregion
	}
}