using System;
using System.IO;
using System.Collections.Generic;

using CmisSync.Lib.Storage;

namespace CmisSync.Lib.Data
{
    [Serializable]
    public class MappedFolder : AbstractMappedObject, IMappedFolder
    {

        public IMappedFolder Parent { get; set; }

        private List<IMappedObject> children = new List<IMappedObject> ();

        public List<IMappedObject> Children { get { return children; } set { this.children = value; } }

        public override bool ExistsLocally ()
        {
            return FsFactory.CreateDirectoryInfo(GetLocalPath ()).Exists;
        }

        public virtual string GetLocalPath ()
        {
            if (Parent == null)
            {
                return LocalSyncTargetPath;
            }
            else
            {
                return Path.Combine (Parent.GetLocalPath(), Name);
            }
        }

        public MappedFolder( string localSyncTargetPath, string remoteSyncTargetPath, IFileSystemInfoFactory fsFactory = null)
            : base(localSyncTargetPath, remoteSyncTargetPath, fsFactory)
        {
            Name = FsFactory.CreateDirectoryInfo(localSyncTargetPath).Name;
        }

        public MappedFolder ( IMappedFolder parent, string name, IFileSystemInfoFactory fsFactory = null)
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

        public virtual string GetRemotePath ()
        {
            if(Parent == null)
            {
                return RemoteSyncTargetPath;
            }
            else
            {
                string path = Parent.GetRemotePath();
                return path + (path.EndsWith("/")? "" : "/") + Name;
            }
        }

    }

}

