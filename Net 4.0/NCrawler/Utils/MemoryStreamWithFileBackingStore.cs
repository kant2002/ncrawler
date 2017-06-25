using System;
using System.IO;

using NCrawler.Extensions;

namespace NCrawler.Utils
{
	/// <summary>
	/// 	Used for writing data to memory stream, but if data gets to large, writes dta to disk storage
	/// </summary>
	public class MemoryStreamWithFileBackingStore : Stream
	{
		#region Fields

		private MemoryStream m_MemoryStream = new MemoryStream();
		private long bytesWritten;
		private FileStream m_FileStoreStream;
		private readonly int m_BufferSize;
		private TempFile m_TempFile;
		private byte[] m_Data;

		#endregion

		#region Constructors

		public MemoryStreamWithFileBackingStore(int contentLength, long maxBytesInMemory, int bufferSize)
		{
            this.m_BufferSize = bufferSize;
			if (contentLength > maxBytesInMemory)
			{
                this.m_TempFile = new TempFile();
                this.m_FileStoreStream = this.m_FileStoreStream = new FileStream(this.m_TempFile.FileName, FileMode.Create, FileAccess.Write, FileShare.Write, this.m_BufferSize);
			}
			else
			{
                this.m_MemoryStream = new MemoryStream(contentLength < 0 ? this.m_BufferSize : contentLength);
			}
		}

		#endregion

		#region Instance Properties

		public override bool CanRead
		{
			get { return false; }
		}

		public override bool CanSeek
		{
			get { return false; }
		}

		public override bool CanWrite
		{
			get { return true; }
		}

		public override long Length
		{
			get { throw new NotImplementedException(); }
		}

		public override long Position
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		#endregion

		#region Instance Methods

		public override void Flush()
		{
			throw new NotImplementedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
            this.bytesWritten += count;
			if (this.m_MemoryStream != null)
			{
                this.m_MemoryStream.Write(buffer, offset, count);
			}
			else
			{
                this.m_FileStoreStream.Write(buffer, offset, count);
			}
		}

		public void FinishedWriting()
		{
            if (this.m_MemoryStream != null)
            {
                this.m_Data = this.m_MemoryStream.ToArray();
                this.m_MemoryStream.Dispose();
                this.m_MemoryStream = null;
            }

            if (this.m_FileStoreStream != null)
            {
                this.m_FileStoreStream.Dispose();
                this.m_FileStoreStream = null;
            }
        }

        public Stream GetReaderStream()
		{
			if (!this.m_Data.IsNull())
			{
				return new MemoryStream(this.m_Data);
			}

			return new FileStream(this.m_TempFile.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, this.m_BufferSize);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				FinishedWriting();

				if (!this.m_TempFile.IsNull())
				{
                    this.m_TempFile.Dispose();
                    this.m_TempFile = null;
				}
			}
		}

		#endregion
	}
}