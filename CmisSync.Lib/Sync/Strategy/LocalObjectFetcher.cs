namespace CmisSync.Lib.Sync.Strategy
{
    using System;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    
    public class LocalObjectFetcher : SyncEventHandler
    {
        private IFileSystemInfoFactory fsFactory;

        private IPathMatcher matcher;

        public override bool Handle(ISyncEvent e) {
            if(e is FolderEvent){
                var folderEvent = e as FolderEvent;
                if(folderEvent.LocalFolder != null) {
                    return false;
                }
                string localPath = matcher.CreateLocalPath(folderEvent.RemoteFolder.Path);
                folderEvent.LocalFolder = fsFactory.CreateDirectoryInfo(localPath);
            }
            if(e is FileEvent){
                var fileEvent = e as FileEvent;
                if(fileEvent.LocalFile != null) {
                    return false;
                }
                string localPath = matcher.CreateLocalPath(fileEvent.RemoteFile.Paths[0]);
                fileEvent.LocalFile = fsFactory.CreateFileInfo(localPath);
            }
            return false;
        }

        public LocalObjectFetcher(IPathMatcher matcher, IFileSystemInfoFactory fsFactory = null) {
            if(matcher == null) {
                throw new ArgumentNullException("matcher can not be null");
            }
            this.matcher = matcher;
            if(fsFactory == null) {
                this.fsFactory = new FileSystemInfoFactory();
            } else {
                this.fsFactory = fsFactory;
            }
        }
    }
}

