using System;
using System.IO;
using System.Collections.Generic;

using CmisSync.Lib.Storage;

namespace CmisSync.Lib.Data
{
    [Serializable]
    public class MappedFolder : AbstractMappedObject
    {

        public MappedFolder Parent { get; set; }

        private List<AbstractMappedObject> children = new List<AbstractMappedObject> ();

        public List<AbstractMappedObject> Children { get { return children; } set { this.children = value; } }

        public override bool ExistsLocally ()
        {
            return FsFactory.CreateDirectoryInfo(GetLocalPath ()).Exists;
        }

        public string GetLocalPath ()
        {
            if (Parent == null)
                return LocalSyncTargetPath;
            else {
                string path = Name;
                MappedFolder p = Parent;
                while (p.Parent != null) {
                    path = Path.Combine (p.Name, path);
                    p = p.Parent;
                }
                return Path.Combine (LocalSyncTargetPath, path);
            }
        }

        public MappedFolder( string localSyncTargetPath, string remoteSyncTargetPath, IFileSystemInfoFactory fsFactory = null)
            : base(localSyncTargetPath, remoteSyncTargetPath, fsFactory)
        {
            Name = FsFactory.CreateDirectoryInfo(LocalSyncTargetPath).Name;
        }

        public MappedFolder ( MappedFolder parent, string name, IFileSystemInfoFactory fsFactory = null)
            : base(parent.LocalSyncTargetPath, parent.RemoteSyncTargetPath, fsFactory)
        {
            if(parent == null)
                throw new ArgumentNullException("Given parent is null");
            if(String.IsNullOrEmpty(name))
                throw new ArgumentException("Given name is null or empty");
            if( fsFactory == null )
                FsFactory = parent.FsFactory;
            Parent = parent;
            Name = name;
        }
    }

}

