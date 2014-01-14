using System;
using System.IO;

using DotCMIS.Client;

namespace CmisSync.Lib.Events
{
    public class FileEvent : AbstractFolderEvent
    {
        public ContentChangeType LocalContent { get; set; }

        public ContentChangeType RemoteContent { get; set; }

        public FileInfo LocalFile { get; protected set; }

        public DirectoryInfo LocalParentDirectory { get; protected set; }

        public IDocument RemoteFile { get; protected set; }

        public FileEvent (FileInfo localFile = null, DirectoryInfo localParentDirectory = null, IDocument remoteFile = null)
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
        public FileInfo OldLocalFile{ get; protected set; }
        public DirectoryInfo OldParentFolder { get; protected set; }
        public string OldRemoteFilePath { get; protected set; }
        public FileMovedEvent(
            FileInfo oldLocalFile = null,
            FileInfo newLocalFile = null,
            DirectoryInfo oldParentFolder = null,
            DirectoryInfo newParentFolder = null,
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

