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
        public MappedObject(MappedObjectData data = null)
        {
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
    }
}
