using System;
using System.Collections.Generic;

namespace CmisSync.Lib.Events.Filter
{
    /// <summary>
    /// Invalid folder name filter.
    /// </summary>
    public class InvalidFolderNameFilter : AbstractFileFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.Filter.InvalidFolderNameFilter"/> class.
        /// </summary>
        /// <param name='queue'>
        /// Queue where filtered events will be reported to.
        /// </param>
        public InvalidFolderNameFilter (ISyncEventQueue queue) : base (queue)
        {
        }

        /// <summary>
        /// Checks the path for containing invalid folder names.
        /// Reports every filtered event to the queue.
        /// </summary>
        /// <returns>
        /// true if the path contains invalid folder names.
        /// </returns>
        /// <param name='e'>
        /// Event which should be reported as filtered, if the path contains invalid folder names.
        /// </param>
        /// <param name='path'>
        /// Path to be checked for containing invalid folder names.
        /// </param>
        private bool checkPath (ISyncEvent e, string path)
        {
            if (Utils.IsInvalidFolderName (path.Replace ("/", "").Replace ("\"", ""), new List<string>())) {
                Queue.AddEvent (new RequestIgnoredEvent (e, source: this));
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Handles the specified events which are containing paths.
        /// If the path contains invalid folder names, true is returned. Otherwise false.
        /// </summary>
        /// <param name='e'>
        /// Events.
        /// </param>
        public override bool Handle (ISyncEvent e)
        {
            FileDownloadRequest request = e as FileDownloadRequest;
            if(request != null) {
                return checkPath (request, request.LocalPath);
            }
            FSEvent fsevent = e as FSEvent;
            if( fsevent != null)
                return checkPath (fsevent, fsevent.Path);
            return false;
        }
    }
}

