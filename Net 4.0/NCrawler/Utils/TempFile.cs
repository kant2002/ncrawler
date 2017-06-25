using System;
using System.Diagnostics;
using System.IO;

using NCrawler.Extensions;

namespace NCrawler.Utils
{
	/// <summary>
	/// TempFile creates a temp file, that deleted itself when garbage collected
	/// </summary>
	public class TempFile : DisposableBase
	{
		#region Constructors

		public TempFile()
		{
            this.FileName = GetTempFileName();
		}

		public TempFile(string fileName)
		{
            this.FileName = fileName;
		}

		#endregion

		#region Instance Properties

		public string FileName { get; set; }
		public string Tag { get; set; }

		#endregion

		#region Instance Methods

		/// <summary>
		/// Do cleanup here
		/// </summary>
		protected override void Cleanup()
		{
			AspectF.Define.
				IgnoreException<Exception>().
				Do(() =>
					{
						var fi = new FileInfo(this.FileName);
						if (!fi.Exists)
						{
							return;
						}

						fi.Attributes = FileAttributes.Normal;
						fi.Delete();
					});
		}

		#endregion

		#region Class Methods

		public static string GetTempFileName()
		{
			return Path.GetTempFileName();
		}

		#endregion
	}
}