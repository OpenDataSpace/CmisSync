using System;
using System.IO;

using DotCMIS.Client;

using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;

using log4net;

namespace CmisSync.Lib.Sync.Strategy { 
    public class FileSystemEventAccumulator : ReportingSyncEventHandler {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(FileSystemEventAccumulator));

        private IMetaDataStorage storage;
        private ISession session;


        public override bool Handle(ISyncEvent e) {
            FileEvent fileEvent = e as FileEvent;
            if(fileEvent != null) {
                string fileName = fileEvent.LocalFile.FullName;
                fileEvent.RemoteFile = session.GetObject(storage.GetFileId(fileName)) as IDocument;
            }
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
