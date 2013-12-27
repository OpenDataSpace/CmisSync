using System;
using CmisSync.Lib.Events;

namespace CmisSync.Lib.Events.Filter
{
    public class IgnoredFilesFilter : AbstractFileFilter
    {
        public IgnoredFilesFilter (SyncEventQueue queue) : base(queue)
        {
            throw new NotImplementedException("IgnoredFilesFilter is not yet implemented, because there is no possibility to ignore excact files");
        }

        public override bool Handle (ISyncEvent e)
        {
            FileDownloadRequest request = e as FileDownloadRequest;
            if(request == null)
                return false;
            //TODO If files could be ignored, they should be filtered out here
            return false;
        }
    }
}

