using System;
using System.IO;

using DotCMIS.Client;
using DotCMIS.Exceptions;

using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;

using log4net;

namespace CmisSync.Lib.Sync.Strategy { 
    public class FileSystemEventAccumulator : ReportingSyncEventHandler {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(FileSystemEventAccumulator));

        private IMetaDataStorage storage;
        private ISession session;

        private ICmisObject GetRemoteObject(ISyncEvent e) {
            if(e is FileEvent) {
                return (e as FileEvent).RemoteFile;
            }
            return (e as FolderEvent).RemoteFolder;
        }

        private void SetRemoteObject(ISyncEvent e, ICmisObject remote) {
            if(e is FileEvent) {
                (e as FileEvent).RemoteFile = remote as IDocument;
            }else{
                (e as FolderEvent).RemoteFolder = remote as IFolder;
            }
        }

        private string FetchIdFromStorage(ISyncEvent e) {
            if(e is FileEvent) {
                string fileName = (e as FileEvent).LocalFile.FullName;
                return storage.GetFileId(fileName);
            }
            string folderName = (e as FolderEvent).LocalFolder.FullName;
            return storage.GetFolderId(folderName);
        }

        public override bool Handle(ISyncEvent e) {
            if(!(e is FileEvent || e is FolderEvent)){
                return false;
            }
            ICmisObject remote = GetRemoteObject(e);
            //already there, no need to GetObject
            if (remote != null) {
                return false;
            }

            string id = FetchIdFromStorage(e);

            try {
                remote = session.GetObject(id);
            } catch (CmisObjectNotFoundException) {
                //Deleted on Server, this is ok
                return false;
            }

            SetRemoteObject(e, remote);
            return false;
        }

        public FileSystemEventAccumulator(ISyncEventQueue queue, ISession session, IMetaDataStorage storage):base(queue) {
            if(session == null)
                throw new ArgumentNullException("Session instance is needed , but was null");
            if(storage == null)
                throw new ArgumentNullException("MetaDataStorage instance is needed, but was null");
            this.session = session;
            this.storage = storage;
        } 


    }
}
