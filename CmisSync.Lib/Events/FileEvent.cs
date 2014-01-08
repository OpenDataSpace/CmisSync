using System;
using System.IO;

using DotCMIS.Client;

namespace CmisSync.Lib.Events
{
    public class FileEvent : AbstractFolderEvent
    {
        public ChangeType LocalContent { get; set; }

        public ChangeType RemoteContent { get; set; }

        public FileEvent (FileInfo localFile = null, DirectoryInfo localParentDirectory = null, IDocument remoteFile = null)
        {
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

