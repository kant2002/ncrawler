using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;

using NCrawler.Extensions;
using NCrawler.Utils;

namespace NCrawler.IsolatedStorageServices
{
	public class IsolatedStorageCrawlerHistoryService : HistoryServiceBase
	{
		#region Constants

		private const string CrawlHistoryName = @"NCrawlHistory";

		#endregion

		#region Readonly & Static Fields

		private readonly Uri m_BaseUri;
		private readonly DictionaryCache m_DictionaryCache = new DictionaryCache(500);
		private readonly bool m_Resume;
		private readonly IsolatedStorageFile m_Store = IsolatedStorageFile.GetMachineStoreForDomain();

		#endregion

		#region Fields

		private long? m_Count;

		#endregion

		#region Constructors

		public IsolatedStorageCrawlerHistoryService(Uri baseUri, bool resume)
		{
            this.m_BaseUri = baseUri;
            this.m_Resume = resume;
			if (!resume)
			{
                this.Clean();
			}
			else
			{
                this.Initialize();
			}
		}

		#endregion

		#region Instance Properties

		private string WorkFolderPath
		{
			get
			{
				var workFolderName = this.m_BaseUri.GetHashCode().ToString();
				return Path.Combine(CrawlHistoryName, workFolderName).Max(20);
			}
		}

		#endregion

		#region Instance Methods

		protected override void Add(string key)
		{
			var path = this.GetFileName(key, true);
			using (var isoFile = new IsolatedStorageFileStream(path, FileMode.Create, this.m_Store))
			{
				using (var sw = new StreamWriter(isoFile))
				{
					sw.Write(key);
				}
			}

            this.m_DictionaryCache.Remove(key);
            this.m_Count = null;
		}

		protected override void Cleanup()
		{
			if (!this.m_Resume)
			{
                this.Clean();
			}

            this.m_DictionaryCache.Dispose();
            this.m_Store.Dispose();
			base.Cleanup();
		}

		protected override bool Exists(string key)
		{
			return AspectF.Define.
				Cache<bool>(this.m_DictionaryCache, key).
				Return(() =>
					{
						var path = this.GetFileName(key, false) + "*";
						var fileNames = this.m_Store.GetFileNames(path);
						foreach (var fileName in fileNames)
						{
							using (var isoFile = new IsolatedStorageFileStream(Path.Combine(this.WorkFolderPath, fileName),
								FileMode.Open, FileAccess.Read, this.m_Store))
							{
								var content = isoFile.ReadToEnd();
								if (content == key)
								{
									return true;
								}
							}
						}

						return false;
					});
		}

		protected override long GetRegisteredCount()
		{
			return this.m_Count ?? this.m_Store.GetFileNames(Path.Combine(this.WorkFolderPath, "*")).Count();
		}

		protected string GetFileName(string key, bool includeGuid)
		{
			var hashString = key.GetHashCode().ToString();
			var fileName = hashString + "_" + (includeGuid ? Guid.NewGuid().ToString() : string.Empty);
			return Path.Combine(this.WorkFolderPath, fileName);
		}

		private void Clean()
		{
			AspectF.Define.
				IgnoreException<DirectoryNotFoundException>().
				IgnoreException<IsolatedStorageException>().
				Do(() =>
					{
						var directoryNames = this.m_Store.GetDirectoryNames(CrawlHistoryName + "\\*");
						var workFolderName = this.WorkFolderPath.Split('\\').Last();
						if (directoryNames.Where(w => w == workFolderName).Any())
						{
                            this.m_Store.
								GetFileNames(Path.Combine(this.WorkFolderPath, "*")).
								ForEach(f => AspectF.Define.
									IgnoreException<IsolatedStorageException>().
									Do(() => this.m_Store.DeleteFile(Path.Combine(this.WorkFolderPath, f))));
                            this.m_Store.DeleteDirectory(this.WorkFolderPath);
						}
					});
            this.Initialize();
		}

		private void Initialize()
		{
			if (!this.m_Store.DirectoryExists(CrawlHistoryName))
			{
                this.m_Store.CreateDirectory(CrawlHistoryName);
			}

			if (!this.m_Store.DirectoryExists(this.WorkFolderPath))
			{
                this.m_Store.CreateDirectory(this.WorkFolderPath);
			}
		}

		#endregion
	}
}