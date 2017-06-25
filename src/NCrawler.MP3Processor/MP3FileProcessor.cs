using System.IO;
using System.Net;
using System.Threading.Tasks;
using HundredMilesSoftware.UltraID3Lib;

using NCrawler.Extensions;
using NCrawler.Interfaces;
using NCrawler.Utils;

namespace NCrawler.MP3Processor
{
	public class Mp3FileProcessor : IPipelineStep
	{
		#region IPipelineStep Members

		public Task ProcessAsync(Crawler crawler, PropertyBag propertyBag)
		{
			if (propertyBag.StatusCode != HttpStatusCode.OK)
			{
				return Task.CompletedTask;
			}

			using (var tempFile = new TempFile())
			{
				using (var fs = new FileStream(tempFile.FileName, FileMode.Create, FileAccess.Write, FileShare.Read, 0x1000))
				using (var input = propertyBag.GetResponse())
				{
					input.CopyToStream(fs);
				}

				var id3 = new UltraID3();
				id3.Read(tempFile.FileName);

				propertyBag["MP3_Album"].Value = id3.Album;
				propertyBag["MP3_Artist"].Value = id3.Artist;
				propertyBag["MP3_Comments"].Value = id3.Comments;
				propertyBag["MP3_Duration"].Value = id3.Duration;
				propertyBag["MP3_Genre"].Value = id3.Genre;
				propertyBag["MP3_Title"].Value = id3.Title;
            }

            return Task.CompletedTask;
        }

		#endregion
	}
}