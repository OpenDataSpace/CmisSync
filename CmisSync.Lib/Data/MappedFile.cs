using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;

using CmisSync.Lib.Storage;

using DotCMIS.Client;

namespace CmisSync.Lib.Data
{

    [Serializable]
    public class MappedFile : MappedObject, IMappedFile
    {


        public MappedFile (MappedObjectData data, IMetaDataStorage storage, IFileSystemInfoFactory fsFactory = null)
            : base(data, storage, fsFactory)
        {
        }
    }
}

