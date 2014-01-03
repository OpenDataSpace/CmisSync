using System;

namespace CmisSync.Lib.Events.Filter
{
    public class InvalidFolderNameFilter : AbstractFileFilter
    {
        public InvalidFolderNameFilter (SyncEventQueue queue) : base (queue)
        {
        }

        private bool checkPath (ISyncEvent e, string path)
        {
            if (Utils.IsInvalidFolderName (path.Replace ("/", "").Replace ("\"", ""))) {
                Queue.AddEvent (new RequestIgnoredEvent (e, source: this));
                return true;
            }
            else
            {
                return false;
            }
        }

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

