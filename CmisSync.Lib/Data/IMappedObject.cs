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
    public interface IMappedObject
    {
        string RemoteObjectId { get; set; }

        string LastChangeToken { get; set; }

        DateTime? LastRemoteWriteTimeUtc { get; set; }

        DateTime? LastLocalWriteTimeUtc { get; set; }

        byte[] LastChecksum { get; set; }

        string ChecksumAlgorithmName { get; set; }

        string RemoteSyncTargetPath { get; }

        string LocalSyncTargetPath { get; }

        string Name { get; set; }

        string Description { get; set; }

        Guid Guid { get; set; }

        bool ExistsLocally ();

        IFileSystemInfoFactory FsFactory { get; }
    }

}
