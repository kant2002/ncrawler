using System;
using System.IO;
using System.Linq;
using System.Threading;

using NCrawler.Extensions;
using NCrawler.Utils;

namespace NCrawler.FileStorageServices
{
	public class FileCrawlQueueService : CrawlerQueueServiceBase
	{
		#region Readonly & Static Fields

		private readonly string m_StoragePath;

		#endregion

		#region Fields

		private long m_Count;

		#endregion

		#region Constructors

		public FileCrawlQueueService(string storagePath, bool resume)
		{
            this.m_StoragePath = storagePath;

			if (!resume)
			{
				Clean();
			}
			else
			{
				Initialize();
                this.m_Count = Directory.GetFiles(this.m_StoragePath).Count();
			}
		}

		#endregion

		#region Instance Methods

		protected override long GetCount()
		{
			return Interlocked.Read(ref this.m_Count);
		}

		protected override CrawlerQueueEntry PopImpl()
		{
#if !DOTNET4
			var fileName = Directory.GetFiles(this.m_StoragePath).FirstOrDefault();
#else
			string fileName = Directory.EnumerateFiles(m_StoragePath).FirstOrDefault();
#endif
			if (fileName.IsNullOrEmpty())
			{
				return null;
			}

			try
			{
				return File.ReadAllText(fileName).FromJson<CrawlerQueueEntry>();
			}
			finally
			{
				File.Delete(fileName);
				Interlocked.Decrement(ref this.m_Count);
			}
		}

		protected override void PushImpl(CrawlerQueueEntry crawlerQueueEntry)
		{
			var data = crawlerQueueEntry.ToJson();
			var fileName = Path.Combine(this.m_StoragePath, Guid.NewGuid().ToString());
			File.WriteAllText(fileName, data);
			Interlocked.Increment(ref this.m_Count);
		}

		protected void Clean()
		{
			AspectF.Define.
				IgnoreException<DirectoryNotFoundException>().
				Do(() => Directory.Delete(this.m_StoragePath, true));

			Initialize();
		}

		/// <summary>
		/// 	Initialize crawler queue
		/// </summary>
		private void Initialize()
		{
			if (!Directory.Exists(this.m_StoragePath))
			{
				Directory.CreateDirectory(this.m_StoragePath);
			}
		}

		#endregion
	}
}