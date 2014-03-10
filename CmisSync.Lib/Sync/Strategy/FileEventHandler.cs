using System;
using System.IO;
using System.Collections.Generic;

using DotCMIS.Client;

using CmisSync.Lib;
using CmisSync.Lib.Events;

namespace CmisSync.Lib.Sync.Strategy
{
    //TODO Is this still needed?
    public class FileEventHandler : ReportingSyncEventHandler
    {
        public static readonly int FILEEVENTHANDLERPRIORITY = 0;
        private Queue<FileEvent> processingQueue = new Queue<FileEvent>();
        private AbstractFileSynchronizer syncer = null;
        private object Lock = new object();
        private IDatabase Db;
        private string RemoteBaseTagetFolder;
        public FileEventHandler (string remoteTargetFolder, IDatabase db, SyncEventQueue queue) : base ( queue)
        {
            if(db == null)
                throw new ArgumentNullException("Given database is null");
            if(String.IsNullOrEmpty(remoteTargetFolder))
                throw new ArgumentException("Remote target folder must not be null or empty");
            this.Db = db;
            this.RemoteBaseTagetFolder = remoteTargetFolder;
        }

        public override int Priority {
            get {
                return FILEEVENTHANDLERPRIORITY;
            }
        }

        public override bool Handle (ISyncEvent e)
        {
            FileMovedEvent movedEvent = e as FileMovedEvent;
            if( movedEvent != null) {
                if(movedEvent.RemoteFile != null && movedEvent.Remote == MetaDataChangeType.MOVED) {
                    // File has been moved remotely, we should move it also locally

                }
                return true;
            }
            FileEvent fileEvent = e as FileEvent;
            if(fileEvent != null) {
                HandleFileEvent(fileEvent);
                return true;
            }
            return false;
        }

        private void HandleFileEvent(FileEvent fileEvent) {
            lock(Lock) {

            }
            processingQueue.Enqueue(fileEvent);
        }

/*        private bool RemotelyMoved(IDocument remoteDocument, out FileInfo oldFile, out FileInfo newFile) {
            string savedPath = Db.GetFilePath(remoteDocument.Id);
            oldFile = new FileInfo(savedPath);
            List<string> localPaths = Cmis.CmisUtils.GetLocalPaths(remoteDocument);
            if(localPaths.Contains(savedPath)) {
                newFile = new FileInfo(savedPath);
                return false;
            } else if(){

                return true;
            }
        }*/
    }
}

