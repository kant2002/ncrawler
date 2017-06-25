using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using EPocalipse.IFilter;

using NCrawler.Extensions;
using NCrawler.Interfaces;
using NCrawler.Utils;

namespace NCrawler.IFilterProcessor
{
	public class IFilterProcessor : IPipelineStepWithTimeout
	{
		#region Readonly & Static Fields

		protected readonly Dictionary<string, string> m_MimeTypeExtensionMapping =
			new Dictionary<string, string>();

		#endregion

		#region Constructors

		public IFilterProcessor()
		{
            this.ProcessorTimeout = TimeSpan.FromSeconds(10);
		}

		public IFilterProcessor(string mimeType, string extension)
			: this()
		{
            this.m_MimeTypeExtensionMapping.Add(mimeType.ToUpperInvariant(), extension);
		}

		#endregion

		#region Instance Methods

		protected virtual string MapContentTypeToExtension(string contentType)
		{
			contentType = contentType.ToLowerInvariant();
			return this.m_MimeTypeExtensionMapping[contentType];
		}

        #endregion

        #region IPipelineStepWithTimeout Members

        public async Task ProcessAsync(Crawler crawler, PropertyBag propertyBag)
        {
            if (propertyBag.StatusCode != HttpStatusCode.OK)
            {
                return;
            }

            var extension = MapContentTypeToExtension(propertyBag.ContentType);
            if (extension.IsNullOrEmpty())
            {
                return;
            }

            propertyBag.Title = propertyBag.Step.Uri.PathAndQuery;
            using (var temp = new TempFile())
            {
                temp.FileName += "." + extension;
                using (var fs = new FileStream(temp.FileName, FileMode.Create, FileAccess.Write, FileShare.Read, 0x1000))
                using (var input = propertyBag.GetResponse())
                {
                    input.CopyTo(fs);
                }

                using (var filterReader = new FilterReader(temp.FileName))
                {
                    var content = await filterReader.ReadToEndAsync();
                    propertyBag.Text = content.Trim();
                }
            }
        }

        public TimeSpan ProcessorTimeout { get; set; }

		#endregion
	}
}