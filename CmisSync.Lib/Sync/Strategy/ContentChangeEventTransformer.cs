using System;
using System.IO;

using DotCMIS.Client;

using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;
using CmisSync.Lib.Data;

using log4net;

namespace CmisSync.Lib.Sync.Strategy { 
    public class ContentChangeEventTransformer : ReportingSyncEventHandler {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ContentChangeEventTransformer));

        private IMetaDataStorage storage;

        private IFileSystemInfoFactory fsFactory;

        public override bool Handle(ISyncEvent e) {
            if(! (e is ContentChangeEvent)) {
                return false;
            }
            var contentChangeEvent = e as ContentChangeEvent;
            if(contentChangeEvent.Type != DotCMIS.Enums.ChangeType.Deleted && contentChangeEvent.CmisObject == null) {
                throw new InvalidOperationException("ERROR, ContentChangeEventAccumulator Missing");
            }
            if(contentChangeEvent.Type == DotCMIS.Enums.ChangeType.Deleted) {
                HandleDeletion(contentChangeEvent);
            }else if(contentChangeEvent.CmisObject is IFolder) {
                HandleAsIFolder(contentChangeEvent);
            }else if(contentChangeEvent.CmisObject is IDocument) {
                HandleAsIDocument(contentChangeEvent);
            }

            return true;
        }

        private void HandleDeletion(ContentChangeEvent contentChangeEvent) {
            Logger.Debug(contentChangeEvent.ObjectId);
            IMappedObject savedObject = storage.GetObjectByRemoteId(contentChangeEvent.ObjectId);
            if(savedObject != null)
            {
                IMappedObject obj = savedObject as IMappedObject;
                if(obj != null)
                {
                    if(obj.Type == MappedObjectType.Folder)
                    {
                        var dirInfo = fsFactory.CreateDirectoryInfo(storage.GetLocalPath(obj));
                        Queue.AddEvent(new FolderEvent(dirInfo, null) {Remote = MetaDataChangeType.DELETED});
                        return;
                    }
                    else
                    {
                        var fileInfo = fsFactory.CreateFileInfo(storage.GetLocalPath(obj));
                        Queue.AddEvent(new FileEvent(fileInfo, fileInfo.Directory, null) {Remote = MetaDataChangeType.DELETED});
                        return;
                    }
                }
            }
            Logger.Debug("nothing found in local storage; it has never been synced");
        }

        private void HandleAsIDocument(ContentChangeEvent contentChangeEvent){
            IDocument doc = contentChangeEvent.CmisObject as IDocument;
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
                        IMappedObject file = storage.GetObjectByRemoteId(doc.Id);
                        var fileInfo = (file == null) ? null : fsFactory.CreateFileInfo(storage.GetLocalPath(file));
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
                        IMappedObject file = storage.GetObjectByRemoteId(doc.Id);
                        var fileInfo = (file == null) ? null : fsFactory.CreateFileInfo(storage.GetLocalPath(file));
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

            IMappedObject dir = storage.GetObjectByRemoteId(folder.Id);
            IDirectoryInfo dirInfo = (dir == null) ? null : fsFactory.CreateDirectoryInfo(storage.GetLocalPath(dir));
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

        public ContentChangeEventTransformer(ISyncEventQueue queue, IMetaDataStorage storage, IFileSystemInfoFactory fsFactory = null): base(queue) {
            
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
