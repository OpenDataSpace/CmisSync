using System;
using System.ComponentModel;
using System.Text;
using System.IO;
using System.Collections.Generic;

using CmisSync.Lib;
using CmisSync.Lib.Storage;

using DotCMIS.Client;

namespace CmisSync.Lib.Data
{
    [Serializable]
    public class MappedObject : MappedObjectData, IMappedObject
    {
        public MappedObject(MappedObjectData data, IMetaDataStorage storage, IFileSystemInfoFactory fsFactory = null) {
            if (storage == null)
            {
                throw new ArgumentNullException("given storage is null");
            }

            this.Storage = storage;
            if (fsFactory == null)
            {
                this.FsFactory = new FileSystemInfoFactory();
            }
            else
            {
                this.FsFactory = fsFactory;
            }

            if(data != null)
            {
                this.ParentId = data.ParentId;
                this.Description = data.Description;
                this.ChecksumAlgorithmName = data.ChecksumAlgorithmName;
                this.Guid = data.Guid;
                this.LastChangeToken = data.LastChangeToken;
                this.LastLocalWriteTimeUtc = data.LastLocalWriteTimeUtc;
                this.LastRemoteWriteTimeUtc = data.LastRemoteWriteTimeUtc;
                this.Name = data.Name;
                this.RemoteObjectId = data.RemoteObjectId;
                this.Type = data.Type;
                this.LastContentSize = data.LastContentSize;
            }
        }

        public IMappedFolder Parent { get; set; }

        public IFileSystemInfoFactory FsFactory { get; protected set; }

        protected IMetaDataStorage Storage { get; private set; }

        public string RemoteSyncTargetPath {
            get
            {
                return this.Storage.GetRemotePath(this);
            }
        }

        public string LocalSyncTargetPath {
            get
            {
                return this.Storage.GetLocalPath(this);
            }
        }

        public virtual bool ExistsLocally()
        {
            switch(this.Type)
            {
            case MappedObjectType.File:
                return this.FsFactory.CreateFileInfo(this.LocalSyncTargetPath).Exists;
            case MappedObjectType.Folder:
                return this.FsFactory.CreateDirectoryInfo(this.LocalSyncTargetPath).Exists;
            case MappedObjectType.Unkown:
                goto default;
            default:
                throw new ArgumentException(string.Format("ExistsLocally is not implemented for MappedObjectType: {0}", this.Type.ToString()));
            }
        }
    }
}
