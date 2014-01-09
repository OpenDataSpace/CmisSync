using System;
using System.IO;

using DotCMIS.Client;

namespace CmisSync.Lib.Events
{
    public class FileEvent : AbstractFolderEvent
    {
        public ChangeType LocalContent { get; set; }

        public ChangeType RemoteContent { get; set; }

        public FileInfo LocalFile { get; private set; }

        public DirectoryInfo LocalParentDirectory { get; private set; }

        public IDocument RemoteFile { get; private set; }

        public FileEvent (FileInfo localFile = null, DirectoryInfo localParentDirectory = null, IDocument remoteFile = null)
        {
            if( localFile == null && remoteFile == null)
                throw new ArgumentNullException("Given local or remote file must not be null");
            this.LocalFile = localFile;
            this.LocalParentDirectory = localParentDirectory;
            this.RemoteFile = remoteFile;
            this.LocalContent = ChangeType.NONE;
            this.RemoteContent = ChangeType.NONE;
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
}

