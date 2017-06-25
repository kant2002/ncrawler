using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading;

using NCrawler.Extensions;
using NCrawler.Utils;

namespace NCrawler.IsolatedStorageServices
{
	public class IsolatedStorageCrawlerQueueService : CrawlerQueueServiceBase
	{
		#region Constants

		private const string NCrawlerQueueDirectoryName = "NCrawler";

		#endregion

		#region Readonly & Static Fields

		private readonly Uri m_CrawlStart;
		private readonly IsolatedStorageFile m_Store;

		#endregion

		#region Fields

		private long m_Count;

		#endregion

		#region Constructors

		public IsolatedStorageCrawlerQueueService(Uri crawlStart, bool resume)
		{
            this.m_CrawlStart = crawlStart;
            this.m_Store = IsolatedStorageFile.GetMachineStoreForDomain();

			if (!resume)
			{
				Clean();
			}
			else
			{
				Initialize();
                this.m_Count = this.m_Store.GetFileNames(Path.Combine(this.WorkFolderPath, "*")).Count();
			}
		}

		#endregion

		#region Instance Properties

		private string WorkFolderPath
		{
			get
			{
				var workFolderName = this.m_CrawlStart.GetHashCode().ToString();
				return Path.Combine(NCrawlerQueueDirectoryName, workFolderName).Max(20);
			}
		}

		#endregion

		#region Instance Methods

		protected override void Cleanup()
		{
			Clean();
            this.m_Store.Dispose();
			base.Cleanup();
		}

		protected override long GetCount()
		{
			return Interlocked.Read(ref this.m_Count);
		}

		protected override CrawlerQueueEntry PopImpl()
		{
			var fileName = this.m_Store.GetFileNames(Path.Combine(this.WorkFolderPath, "*")).FirstOrDefault();
			if (fileName.IsNullOrEmpty())
			{
				return null;
			}

			var path = Path.Combine(this.WorkFolderPath, fileName);
			try
			{
				using (var isoFile =
					new IsolatedStorageFileStream(path, FileMode.Open, this.m_Store))
				{
                    using (var reader = new StreamReader(isoFile))
                    {
                        return reader.ReadToEnd().FromJson<CrawlerQueueEntry>();
                    }
				}
			}
			finally
			{
                this.m_Store.DeleteFile(path);
				Interlocked.Decrement(ref this.m_Count);
			}
		}

		protected override void PushImpl(CrawlerQueueEntry crawlerQueueEntry)
		{
			var data = crawlerQueueEntry.ToJson();
			var path = Path.Combine(this.WorkFolderPath, Guid.NewGuid().ToString());
			using (var isoFile = new IsolatedStorageFileStream(path, FileMode.Create, this.m_Store))
			{
                using (var writer = new StreamWriter(isoFile))
                {
                    writer.WriteLine(data);
                }
			}

			Interlocked.Increment(ref this.m_Count);
		}

		protected void Clean()
		{
			AspectF.Define.
				IgnoreExceptions().
				Do(() =>
					{
						var directoryNames = this.m_Store.GetDirectoryNames(NCrawlerQueueDirectoryName + "\\*");
						var workFolderName = this.WorkFolderPath.Split('\\').Last();
						if (directoryNames.Where(w => w == workFolderName).Any())
						{
                            this.m_Store.
								GetFileNames(Path.Combine(this.WorkFolderPath, "*")).
								ForEach(f => this.m_Store.DeleteFile(Path.Combine(this.WorkFolderPath, f)));
                            this.m_Store.DeleteDirectory(this.WorkFolderPath);
						}
					});
			Initialize();
		}

		/// <summary>
		/// 	Initialize crawler queue
		/// </summary>
		private void Initialize()
		{
			if (!this.m_Store.DirectoryExists(NCrawlerQueueDirectoryName))
			{
                this.m_Store.CreateDirectory(NCrawlerQueueDirectoryName);
			}

			if (!this.m_Store.DirectoryExists(this.WorkFolderPath))
			{
                this.m_Store.CreateDirectory(this.WorkFolderPath);
			}
		}

		#endregion
	}
}