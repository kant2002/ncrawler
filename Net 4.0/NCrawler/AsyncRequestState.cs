using System;
using System.Net;
using NCrawler.Events;
using NCrawler.Services;
using NCrawler.Utils;

namespace NCrawler
{
    public class AsyncRequestState<T>
    {
        #region Instance Properties

        public CrawlStep CrawlStep { get; set; }
        public Exception Exception { get; set; }
        public DownloadMethod Method { get; set; }
        public PropertyBag PropertyBag { get; set; }

        /// <summary>
        /// Sets or sets date and time when request was started.
        /// </summary>
        public DateTime StartTime { get; set; }
        public CrawlStep Referrer { get; set; }
        public Action<AsyncRequestState<T>> Complete { private get; set; }
        public Action<DownloadProgressEventArgs> DownloadProgress { get; set; }

        public HttpWebRequest Request { get; set; }
        public MemoryStreamWithFileBackingStore ResponseBuffer { get; set; }
        public int Retry { get; set; }
        public T State { get; set; }

        #endregion

        #region Instance Methods

        public void CallComplete(PropertyBag propertyBag, Exception exception)
        {
            Clean();

            PropertyBag = propertyBag;
            Exception = exception;
            Complete(this);
        }

        public void Clean()
        {
            if (ResponseBuffer != null)
            {
                ResponseBuffer.Close();
                ResponseBuffer = null;
            }
        }

        #endregion
    }
}
