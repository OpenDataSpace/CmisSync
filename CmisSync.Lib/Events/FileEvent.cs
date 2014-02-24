using System;
using System.IO;

using DotCMIS.Client;

using CmisSync.Lib.Storage;

namespace CmisSync.Lib.Events
{
    public class FileEvent : AbstractFolderEvent

    {
        public ContentChangeType LocalContent { get; set; }

        public ContentChangeType RemoteContent { get; set; }

        public IFileInfo LocalFile { get; protected set; }

        public IDirectoryInfo LocalParentDirectory { get; protected set; }

        public IDocument RemoteFile { get; protected set; }

        public FileEvent (IFileInfo localFile = null, IDirectoryInfo localParentDirectory = null, IDocument remoteFile = null)
        {
            if( localFile == null && remoteFile == null)
                throw new ArgumentNullException("Given local or remote file must not be null");
            this.LocalFile = localFile;
            this.LocalParentDirectory = localParentDirectory;
            this.RemoteFile = remoteFile;
            this.LocalContent = ContentChangeType.NONE;
            this.RemoteContent = ContentChangeType.NONE;
        }


        public override string ToString ()
        {
            return string.Format ("[FileEvent: Local={0}, LocalContent={1}, Remote={2}, RemoteContent={3}]",
                                  Local,
                                  LocalContent,
                                  Remote,
                                  RemoteContent);
        }
    }

    public class FileMovedEvent : FileEvent
    {
        public IFileInfo OldLocalFile{ get; protected set; }
        public IDirectoryInfo OldParentFolder { get; protected set; }
        public string OldRemoteFilePath { get; protected set; }
        public FileMovedEvent(
            IFileInfo oldLocalFile = null,
            IFileInfo newLocalFile = null,
            IDirectoryInfo oldParentFolder = null,
            IDirectoryInfo newParentFolder = null,
            string oldRemoteFilePath = null,
            IDocument newRemoteFile = null
        ) : base(newLocalFile, newParentFolder, newRemoteFile) {
            Local = MetaDataChangeType.MOVED;
            OldLocalFile = oldLocalFile;
            OldParentFolder = oldParentFolder;
            OldRemoteFilePath = oldRemoteFilePath;
        }
    }
}

