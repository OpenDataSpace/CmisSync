
namespace CmisSync.Lib.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Storage;

    [Serializable]
    public class MappedFolder : AbstractMappedObject, IMappedFolder
    {
        public IMappedFolder Parent { get; set; }

        private List<IMappedObject> children = new List<IMappedObject>();

        public List<IMappedObject> Children { get { return children; } set { this.children = value; } }

        public override bool ExistsLocally()
        {
            return this.FsFactory.CreateDirectoryInfo(this.GetLocalPath()).Exists;
        }

        public virtual string GetLocalPath()
        {
            if (this.Parent == null)
            {
                return this.LocalSyncTargetPath;
            }
            else
            {
                return Path.Combine(this.Parent.GetLocalPath(), this.Name);
            }
        }

        public MappedFolder( string localSyncTargetPath, string remoteSyncTargetPath, IFileSystemInfoFactory fsFactory = null)
            : base(localSyncTargetPath, remoteSyncTargetPath, fsFactory)
        {
            this.Name = this.FsFactory.CreateDirectoryInfo(localSyncTargetPath).Name;
        }

        public MappedFolder( IMappedFolder parent, string name, IFileSystemInfoFactory fsFactory = null)
            : base(parent.LocalSyncTargetPath, parent.RemoteSyncTargetPath, fsFactory)
        {
            if (parent == null)
            {
                throw new ArgumentNullException("Given parent is null");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Given name is null or empty");
            }

            if (fsFactory == null)
            {
                this.FsFactory = parent.FsFactory;
            }

            this.Parent = parent;
            this.Name = name;
        }

        public virtual string GetRemotePath()
        {
            if (this.Parent == null)
            {
                return this.RemoteSyncTargetPath;
            }
            else
            {
                string path = this.Parent.GetRemotePath();
                return path + (path.EndsWith("/") ? string.Empty : "/") + this.Name;
            }
        }
    }
}
