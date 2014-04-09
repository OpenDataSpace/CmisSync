using System;
using System.IO;

using DotCMIS.Client;

using CmisSync.Lib.Storage;

namespace CmisSync.Lib.Events
{
    public class FolderEvent : AbstractFolderEvent
    {
        public bool Recursive { get; set; }

        public IDirectoryInfo LocalFolder { get; set; }

        public IFolder RemoteFolder { get; set; }

        public FolderEvent (IDirectoryInfo localFolder = null, IFolder remoteFolder = null)
        {
            if(localFolder == null && remoteFolder == null)
                throw new ArgumentNullException("One of the given folders must not be null");
            Recursive = false;
            LocalFolder = localFolder;
            RemoteFolder = remoteFolder;
        }

        public override string ToString ()
        {
            return string.Format ("[FolderEvent: Local={0} on {2}, Remote={1} on {3}]",
                                  Local,
                                  Remote,
                                  LocalFolder != null ? LocalFolder.Name : "",
                                  RemoteFolder!= null ? RemoteFolder.Name : "");
        }
    }

    public class FolderMovedEvent : FolderEvent
    {
        public IDirectoryInfo OldLocalFolder { get; private set; }
        public string OldRemoteFolderPath { get; private set; }
        public FolderMovedEvent(
            IDirectoryInfo oldLocalFolder,
            IDirectoryInfo newLocalFolder,
            string oldRemoteFolderPath,
            IFolder newRemoteFolder) : base ( newLocalFolder, newRemoteFolder) {
            OldLocalFolder = oldLocalFolder;
            OldRemoteFolderPath = oldRemoteFolderPath;
        }
    }

    public abstract class AbstractFolderEvent : ISyncEvent
    {
        public MetaDataChangeType Local { get; set; }

        public MetaDataChangeType Remote { get; set; }

        public AbstractFolderEvent ()
        {
            this.Local = MetaDataChangeType.NONE;
            this.Remote = MetaDataChangeType.NONE;
        }
    }
}

