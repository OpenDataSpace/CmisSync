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

        string ParentId { get; set; }

        string LastChangeToken { get; set; }

        DateTime? LastRemoteWriteTimeUtc { get; set; }

        DateTime? LastLocalWriteTimeUtc { get; set; }

        byte[] LastChecksum { get; set; }

        string ChecksumAlgorithmName { get; set; }

        string Name { get; set; }

        string Description { get; set; }

        Guid Guid { get; set; }

        MappedObjectType Type { get; }
    }

}
