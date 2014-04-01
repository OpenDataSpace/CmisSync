using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;

using CmisSync.Lib.Storage;

using DotCMIS.Client;

namespace CmisSync.Lib.Data
{

    [Serializable]
    public class MappedFile : AbstractMappedObject
    {
        public List<MappedFolder> Parents { get; set; }

        [DefaultValue(-1)]
        public long LastFileSize { get; set; }

        public MappedFile (MappedFolder parent, IFileSystemInfoFactory fsFactory = null, params MappedFolder[] parents)
            : base(parent.LocalSyncTargetPath, parent.RemoteSyncTargetPath, fsFactory)
        {
            Parents = new List<MappedFolder>();
            Parents.Add (parent);
            if (parents != null)
                Parents.AddRange (parents);
        }

        public override bool ExistsLocally ()
        {
            if (Parents.Count != 1)
                throw new ArgumentOutOfRangeException (String.Format ("Only if one parent exists, this method could return the corect answer, but there are {0} parents", Parents.Count));
            return FsFactory.CreateFileInfo(Path.Combine (Parents [0].GetLocalPath (), Name)).Exists;
        }

        public bool ExistsLocally (MappedFolder parent)
        {
            if (this.Parents.Contains (parent))
                return FsFactory.CreateFileInfo(parent.GetLocalPath ()).Exists;
            else 
                return false;
        }

        public string GetLocalPath (MappedFolder parent)
        {
            return Path.Combine (parent.GetLocalPath (), Name);
        }

        public string GetLocalPath ()
        {
            return Path.Combine (Parents [0].GetLocalPath (), Name);
        }
    }
}

