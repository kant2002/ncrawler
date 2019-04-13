using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NCrawler.Extensions;
using NCrawler.Utils;

namespace NCrawler.FileStorageServices
{
	public class FileCrawlHistoryService : HistoryServiceBase
	{
		#region Readonly & Static Fields

		private readonly DictionaryCache m_DictionaryCache = new DictionaryCache(500);
		private readonly string m_StoragePath;
		private readonly bool m_Resume;

		#endregion

		#region Fields

		private long? m_Count;

		#endregion

		#region Constructors

		public FileCrawlHistoryService(string storagePath, bool resume)
		{
            this.m_StoragePath = storagePath;
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

		#region Instance Methods

		protected override void Add(string key)
		{
			var path = Path.Combine(this.m_StoragePath, this.GetFileName(key, true));
			File.WriteAllText(path, key);
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
			base.Cleanup();
		}

		protected override bool Exists(string key)
		{
			return AspectF.Define.
				Cache<bool>(this.m_DictionaryCache, key).
				Return(() =>
					{
#if DOTNET4
						IEnumerable<string> fileNames = Directory.EnumerateFiles(m_StoragePath, GetFileName(key, false) + "*");
						return fileNames.Select(File.ReadAllText).Any(content => content == key);
#else
					var fileNames = Directory.GetFiles(this.m_StoragePath, this.GetFileName(key, false) + "*");
					return fileNames.Select(fileName => File.ReadAllText(fileName)).Any(content => content == key);
#endif
					});
		}

		protected override long GetRegisteredCount()
		{
			if (this.m_Count.HasValue)
			{
				return this.m_Count.Value;
			}

#if !DOTNET4
            this.m_Count = Directory.GetFiles(this.m_StoragePath).Count();
#else
			m_Count = Directory.EnumerateFiles(m_StoragePath).Count();
#endif
			return this.m_Count.Value;
		}

		protected string GetFileName(string key, bool includeGuid)
		{
			var hashString = key.GetHashCode().ToString();
			return hashString + "_" + (includeGuid ? Guid.NewGuid().ToString() : string.Empty);
		}

		private void Clean()
		{
			AspectF.Define.
				IgnoreException<DirectoryNotFoundException>().
				Do(() => Directory.Delete(this.m_StoragePath, true));

            this.Initialize();
		}

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