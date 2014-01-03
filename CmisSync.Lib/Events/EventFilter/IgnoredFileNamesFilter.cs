using System;
using System.IO;

namespace CmisSync.Lib.Events.Filter
{
    public class IgnoredFileNamesFilter : AbstractFileFilter
    {
        public IgnoredFileNamesFilter(SyncEventQueue queue) : base(queue) { }


        private bool checkFile(ISyncEvent e, string fileName) {
            if(!Utils.WorthSyncing(fileName)) {
                Queue.AddEvent(new RequestIgnoredEvent(e));
                return true;
            }
            return false;
        }

        public override bool Handle (ISyncEvent e)
        {
            FileDownloadRequest request = e as FileDownloadRequest;
            if( request != null)
                return checkFile(request, request.Document.Name);

            FSEvent fsevent = e as FSEvent;
            if(fsevent!=null)
            {
                if(!fsevent.IsDirectory())
                {
                    return checkFile(fsevent, Path.GetFileName(fsevent.Path));
                }
            }

            return false;
        }
    }
}

