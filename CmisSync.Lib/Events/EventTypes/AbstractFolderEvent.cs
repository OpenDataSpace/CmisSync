using System;
using System.IO;

using DotCMIS.Client;

using CmisSync.Lib.Storage;

namespace CmisSync.Lib.Events
{
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

