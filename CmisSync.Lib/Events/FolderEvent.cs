using System;
using System.IO;

using DotCMIS.Client;

namespace CmisSync.Lib.Events
{
    public class FolderEvent : AbstractFolderEvent
    {
        public bool Recursive { get; set; }
        public FolderEvent (DirectoryInfo localFolder = null, IFolder remoteFolder = null)
        {
        }

        public override string ToString ()
        {
            return string.Format ("[FolderEvent: Local={0}, Remote={1}]", Local, Remote);
        }
    }

    public abstract class AbstractFolderEvent : ISyncEvent
    {
        public ChangeType Local {get; set;}
        public ChangeType Remote {get; set;}
    }
}

