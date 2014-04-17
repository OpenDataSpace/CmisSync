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
            var folderEvent = e as FolderEvent;
            string localPath = matcher.CreateLocalPath(folderEvent.RemoteFolder.Path);
            folderEvent.LocalFolder = fsFactory.CreateDirectoryInfo(localPath);
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

