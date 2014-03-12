using System;
using System.IO;

using DotCMIS.Client;

using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;

using log4net;

namespace CmisSync.Lib.Sync.Strategy { 
    public class ContentChangeEventTransformer : ReportingSyncEventHandler {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ContentChangeEventTransformer));
        public static readonly int DEFAULT_PRIORITY = 1000;

        private IMetaDataStorage storage;

        private IFileSystemInfoFactory fsFactory;
        public override int Priority {
            get {
                return DEFAULT_PRIORITY;
            }
        }

        public override bool Handle(ISyncEvent e) {
            if(! (e is ContentChangeEvent)) {
                return false;
            }
            var contentChangeEvent = e as ContentChangeEvent;
            if(contentChangeEvent.Type != DotCMIS.Enums.ChangeType.Deleted && contentChangeEvent.CmisObject == null){
                throw new InvalidOperationException("ERROR, ContenChangeEventAccumulator Missing");
            }
            if(contentChangeEvent.Type == DotCMIS.Enums.ChangeType.Deleted){
                HandleDeletion(contentChangeEvent);
            }else if(contentChangeEvent.CmisObject is IFolder) {
                HandleAsIFolder(contentChangeEvent);
            }else if(contentChangeEvent.CmisObject is IDocument) {
                HandleAsIDocument(contentChangeEvent);
            }

            return true;
        }

        private void HandleDeletion(ContentChangeEvent contentChangeEvent) {
            string path = storage.GetFilePath(contentChangeEvent.ObjectId);
            if(path != null)
            {
                var fileInfo = fsFactory.CreateFileInfo(path);
                Queue.AddEvent(new FileEvent(fileInfo, fileInfo.Directory, null) {Remote = MetaDataChangeType.DELETED});
                return;
            }
            path = storage.GetFolderPath(contentChangeEvent.ObjectId);
            if(path != null)
            {
                var dirInfo = fsFactory.CreateDirectoryInfo(path);
                Queue.AddEvent(new FolderEvent(dirInfo, null) {Remote = MetaDataChangeType.DELETED});
                return;
            }
            //If nothing found in local storage it has never been synced -> nop
        }

        private void HandleAsIDocument(ContentChangeEvent contentChangeEvent){
            IDocument doc = contentChangeEvent.CmisObject as IDocument;
            Logger.Debug("asdasdasd"+doc.Id);
            switch(contentChangeEvent.Type)
            {
                case DotCMIS.Enums.ChangeType.Created:
                    {
                        var fileEvent = new FileEvent(null, null, doc) {Remote = MetaDataChangeType.CREATED};
                        fileEvent.RemoteContent = doc.ContentStreamId == null ? ContentChangeType.NONE : ContentChangeType.CREATED;
                        Queue.AddEvent(fileEvent);
                        break;
                    }
                case DotCMIS.Enums.ChangeType.Security:
                    {
                        string path = storage.GetFilePath(doc.Id);
                        var fileInfo = (path == null) ? null : fsFactory.CreateFileInfo(path);
                        var fileEvent = new FileEvent(fileInfo, fileInfo == null ? null : fileInfo.Directory, doc);
                        if( fileInfo != null )
                        {
                            fileEvent.Remote = MetaDataChangeType.CHANGED;
                        } else {
                            fileEvent.Remote = MetaDataChangeType.CREATED;
                            fileEvent.RemoteContent = ContentChangeType.CREATED;
                        }
                        Queue.AddEvent(fileEvent);
                        break;
                    }
                case DotCMIS.Enums.ChangeType.Updated:
                    {
                        string path = storage.GetFilePath(doc.Id);
                        var fileInfo = (path == null) ? null : fsFactory.CreateFileInfo(path);
                        var fileEvent = new FileEvent(fileInfo, fileInfo == null ? null : fileInfo.Directory, doc);
                        if(fileInfo != null)
                        {
                            fileEvent.Remote = MetaDataChangeType.CHANGED;
                            fileEvent.RemoteContent = ContentChangeType.CHANGED;
                        } else {
                            fileEvent.Remote = MetaDataChangeType.CREATED;
                            fileEvent.RemoteContent = ContentChangeType.CREATED;
                        }
                        Queue.AddEvent(fileEvent);
                        break;
                    }
            }
        }

        private void HandleAsIFolder(ContentChangeEvent contentChangeEvent){
            IFolder folder = contentChangeEvent.CmisObject as IFolder;

            string path = storage.GetFolderPath(folder.Id);
            IDirectoryInfo dirInfo = (path != null) ? fsFactory.CreateDirectoryInfo(path) : null;
            var folderEvent = new FolderEvent(dirInfo, folder);
            switch(contentChangeEvent.Type)
            {
                case DotCMIS.Enums.ChangeType.Created:
                    folderEvent.Remote = MetaDataChangeType.CREATED;
                    break;
                case DotCMIS.Enums.ChangeType.Updated:
                    folderEvent.Remote = MetaDataChangeType.CHANGED;
                    break;
                case DotCMIS.Enums.ChangeType.Security:
                    folderEvent.Remote = MetaDataChangeType.CHANGED;
                    break;
            }
            Queue.AddEvent(folderEvent);
        }

        public ContentChangeEventTransformer(ISyncEventQueue queue, IMetaDataStorage storage, FileSystemInfoFactory fsFactory = null): base(queue) {
            
            if(storage == null)
                throw new ArgumentNullException("Storage instance is needed for the ContentChangeEventTransformer, but was null");
            this.storage = storage;

            if(fsFactory == null){
                this.fsFactory = new FileSystemInfoFactory();
            }else{
                this.fsFactory = fsFactory;
            }
        }

    }
}
