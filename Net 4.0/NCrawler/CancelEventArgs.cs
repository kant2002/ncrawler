namespace System.ComponentModel
{
    using System;

    /// <summary>
    /// Provides data for a cancelable event.
    /// </summary>
    public class CancelEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the System.ComponentModel.CancelEventArgs class
        /// with the System.ComponentModel.CancelEventArgs.Cancel property set to false.
        /// </summary>
        public CancelEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the System.ComponentModel.CancelEventArgs class
        /// with the System.ComponentModel.CancelEventArgs.Cancel property set to the given
        /// value.
        /// </summary>
        /// <param name="cancel">true to cancel the event; otherwise, false.</param>
        public CancelEventArgs(bool cancel)
        {
            this.Cancel = cancel;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the event should be canceled.
        /// </summary>
        /// <value>true if the event should be canceled; otherwise, false.</value>
        public bool Cancel { get; set; }
    }
}
