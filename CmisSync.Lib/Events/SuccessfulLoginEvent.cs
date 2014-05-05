using System;

namespace CmisSync.Lib.Events
{
    /// <summary>
    /// Successful login on a server should add this event to the event queue.
    /// </summary>
    public class SuccessfulLoginEvent : ISyncEvent
    {
        private Uri Url;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.SuccessfulLoginEvent"/> class.
        /// </summary>
        /// <param name="url">URL of the successful connection</param>
        public SuccessfulLoginEvent( Uri url)
        {
            if (url == null)
                throw new ArgumentNullException("Given Url is null");
            Url = url;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.SuccessfulLoginEvent"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.SuccessfulLoginEvent"/>.</returns>
        public override string ToString ()
        {
            return string.Format ("[SuccessfulLoginEvent {0}]", Url.ToString());
        }
    }
}

