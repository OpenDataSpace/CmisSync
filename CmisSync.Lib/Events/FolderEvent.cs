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
}

