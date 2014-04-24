using System;
using System.IO;

using DotCMIS.Client;

using CmisSync.Lib.Storage;

namespace CmisSync.Lib.Events
{
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
}

