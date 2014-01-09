using System;
using System.IO;

using DotCMIS.Client;

namespace CmisSync.Lib.Events
{
    public class FolderEvent : AbstractFolderEvent
    {
        public bool Recursive { get; set; }

        public DirectoryInfo LocalFolder { get; private set; }

        public IFolder RemoteFolder { get; private set; }

        public FolderEvent (DirectoryInfo localFolder = null, IFolder remoteFolder = null)
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
                                  RemoteFolder.Name);
        }
    }

    public abstract class AbstractFolderEvent : ISyncEvent
    {
        public ChangeType Local { get; set; }

        public ChangeType Remote { get; set; }

        public AbstractFolderEvent ()
        {
            this.Local = ChangeType.NONE;
            this.Remote = ChangeType.NONE;
        }
    }
}

