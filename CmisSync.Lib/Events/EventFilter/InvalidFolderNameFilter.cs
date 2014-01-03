using System;

namespace CmisSync.Lib.Events.Filter
{
    public class InvalidFolderNameFilter : AbstractFileFilter
    {
        public InvalidFolderNameFilter (SyncEventQueue queue) : base (queue)
        {
        }

        public override bool Handle (ISyncEvent e)
        {
            FileDownloadRequest request = e as FileDownloadRequest;
            if(e!=null) {
                if(Utils.IsInvalidFolderName(request.LocalPath.Replace("/", "").Replace("\"",""))) {
                    Queue.AddEvent(new RequestIgnoredEvent(e, source : this));
                    return true;
                }
            }
            return false;
        }
    }
}

