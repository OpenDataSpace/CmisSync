using System;
using System.Collections.Generic;

namespace CmisSync.Lib.Events
{
    /// <summary>
    /// This event should be used by scheduler to periodically start sync processes.
    /// If any inconsitancy is detected, it could also be used by the algorithm itself to force a full sync on the next sync execution.
    /// </summary>
    public class StartNextSyncEvent : ISyncEvent
    {
        private bool fullSyncRequested;

        /// <summary>
        /// Gets a value indicating whether this <see cref="CmisSync.Lib.Events.StartNextSyncEvent"/> should force a full sync.
        /// </summary>
        /// <value>
        /// <c>true</c> if full sync requested; otherwise, <c>false</c>.
        /// </value>
        public bool FullSyncRequested { get {return this.fullSyncRequested; }}

        private Dictionary<string, string> parameters = new Dictionary<string, string>();
        public StartNextSyncEvent (bool fullSyncRequested = false)
        {
            this.fullSyncRequested = fullSyncRequested;
        }

        public void SetParam(string key, string value) {
            this.parameters.Add(key, value);
        }

        public virtual bool TryGetParam(string key, out string value) {
            return this.parameters.TryGetValue(key, out value);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.StartNextSyncEvent"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.StartNextSyncEvent"/>.
        /// </returns>
        public override string ToString ()
        {
            return string.Format ("[StartNextSyncEvent: FullSyncRequested={0}]", FullSyncRequested);
        }
    }
}

