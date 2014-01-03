using System;
using System.Collections.Generic;

namespace CmisSync.Lib.Events.Filter
{
    public class IgnoredFoldersFilter : AbstractFileFilter
    {
        private List<string> ignoredPaths = new List<string>();
        private object ListLock = new object();
        public List<string> IgnoredPaths { set {
                lock(ListLock){
                    this.ignoredPaths = value;
                }
            } }
        public IgnoredFoldersFilter (SyncEventQueue queue) : base(queue) { }

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

