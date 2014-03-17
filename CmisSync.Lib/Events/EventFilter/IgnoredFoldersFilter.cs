using System;
using System.Collections.Generic;

namespace CmisSync.Lib.Events.Filter
{
    /// <summary>
    /// Ignored folders filter.
    /// </summary>
    public class IgnoredFoldersFilter : AbstractFileFilter
    {
        private List<string> ignoredPaths = new List<string>();
        private List<string> wildcards = new List<string>();
        private object ListLock = new object();

        /// <summary>
        /// Sets the ignored paths.
        /// </summary>
        /// <value>
        /// The ignored paths.
        /// </value>
        public List<string> IgnoredPaths { set {
                lock(ListLock){
                    this.ignoredPaths = value;
                }
            } }
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.Filter.IgnoredFoldersFilter"/> class.
        /// </summary>
        /// <param name='queue'>
        /// Queue.
        /// </param>
        public IgnoredFoldersFilter (ISyncEventQueue queue) : base(queue) { }

        public List<string> IgnoreWildcards { set {
                lock(ListLock)
                {
                    this.wildcards = value;
                }
            }
        }

        /// <summary>
        /// Checks the path if it begins with any path, which is ignored. Reports ignores to the queue.
        /// </summary>
        /// <returns>
        /// <c>true</c> if path starts with an ignored path, otherwise <c>false</c> is returned.
        /// </returns>
        /// <param name='e'>
        /// ISyncEvent which is reported to queue, if filtered.
        /// </param>
        /// <param name='localPath'>
        /// The local path which should be checked, if it should be ignored.
        /// </param>
        private bool checkPath (ISyncEvent e, string localPath)
        {
            lock (ListLock) {
                bool result = !String.IsNullOrEmpty (ignoredPaths.Find (delegate (string ignore) {
                    return localPath.StartsWith (ignore);
                }));
                if (result)
                    Queue.AddEvent (new RequestIgnoredEvent (e, source: this));
                return result;
            }
        }

        /// <summary>
        /// Handles FSEvents and FileDownloadRequest events.
        /// If the path starts with an ignored path, <c>true</c> will be returned and an ignored event is added to the queue.
        /// Otherwise <c>false</c> is returned.
        /// </summary>
        /// <param name='e'>
        /// Is checked for FSEvent events and FileDownloadRequest events
        /// </param>
        public override bool Handle (ISyncEvent e)
        {
            FileDownloadRequest request = e as FileDownloadRequest;
            if(request!=null)
                 return checkPath (request, request.LocalPath);
            FSEvent fsevent = e as FSEvent;
            if(fsevent!=null)
                return checkPath(fsevent, fsevent.Path);
            return false;
        }
    }
}

