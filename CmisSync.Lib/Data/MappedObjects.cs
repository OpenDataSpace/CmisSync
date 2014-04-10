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
    public abstract class AbstractMappedObject : IMappedObject
    {
        public AbstractMappedObject( string localSyncTargetPath, string remoteSyncTargetPath, IFileSystemInfoFactory fsFactory = null) {
            if (String.IsNullOrEmpty(localSyncTargetPath))
                throw new ArgumentException("Given local sync target path is null or empty");
            if (String.IsNullOrEmpty(remoteSyncTargetPath))
                throw new ArgumentException("Given remote sync target path is null or empty");
            if (fsFactory == null)
                FsFactory = new FileSystemInfoFactory();
            else
                FsFactory = fsFactory;
            LocalSyncTargetPath = localSyncTargetPath;
            RemoteSyncTargetPath = remoteSyncTargetPath;
        }

        public IFileSystemInfoFactory FsFactory { get; protected set; }

        public virtual string RemoteObjectId { get; set; }

        public virtual string LastChangeToken { get; set; }

        /// <summary>
        /// Gets and sets the time at which the object was last modified.
        /// This is the time on the CMIS server side, in UTC. Client-side time does not matter.
        /// </summary>
        [DefaultValue(null)]
        public virtual DateTime? LastRemoteWriteTimeUtc { get; set; }

        [DefaultValue(null)]
        public virtual DateTime? LastLocalWriteTimeUtc { get; set; }

        public virtual byte[] LastChecksum { get; set; }

        public virtual string ChecksumAlgorithmName { get; set; }

        public virtual string RemoteSyncTargetPath { get; private set; }

        public virtual string LocalSyncTargetPath { get; private set; }

        public virtual string Name { get; set; }

        public virtual string Description { get; set; }

        public abstract bool ExistsLocally ();

        public virtual void Remove()
        {
            throw new NotImplementedException();
        }
    }
}
