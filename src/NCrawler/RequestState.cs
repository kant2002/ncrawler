using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NCrawler.Events;
using NCrawler.Services;
using NCrawler.Utils;

namespace NCrawler
{
	public class RequestState<T>
	{
		#region Instance Properties

		public CrawlStep CrawlStep { get; set; }
		public Exception Exception { get; set; }
		public HttpMethod Method { get; set; }
		public PropertyBag PropertyBag { get; set; }

        /// <summary>
        /// Sets or sets date and time when request was started.
        /// </summary>
        public DateTimeOffset StartTime { get; set; } = DateTimeOffset.UtcNow;
        public CrawlStep Referrer { get; set; }
		public Func<RequestState<T>, Task> Complete { private get; set; }
		public Action<DownloadProgressEventArgs> DownloadProgress { get; set; }
		public Stopwatch DownloadTimer { get; set; }
		public HttpRequestMessage Request { get; set; }
		public MemoryStreamWithFileBackingStore ResponseBuffer { get; set; }
		public int Retry { get; set; }
		public T State { get; set; }

		#endregion

		#region Instance Methods

		public void CallComplete(PropertyBag propertyBag, Exception exception)
		{
            this.Clean();

            this.PropertyBag = propertyBag;
            this.Exception = exception;
            this.Complete(this);
		}

		public void Clean()
		{
			if (this.ResponseBuffer != null)
			{
                this.ResponseBuffer.FinishedWriting();
                this.ResponseBuffer = null;
			}
		}

		#endregion
	}
}