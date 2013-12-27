using System;

namespace CmisSync.Lib.Events.Filter
{
    public class IgnoredFileNamesFilter : AbstractFileFilter
    {
        public IgnoredFileNamesFilter(SyncEventQueue queue) : base(queue) { }

        public override bool Handle (ISyncEvent e)
        {
            FileDownloadRequest request = e as FileDownloadRequest;
            if( request == null)
                return false;
            if(!Utils.WorthSyncing(request.Document.Name)) {
                Queue.AddEvent(new RequestIgnoredEvent(e));
                return true;
            }
            return false;
        }
    }
}

