
namespace CmisSync.Lib.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Storage;

    [Serializable]
    public class MappedFolder : MappedObject, IMappedFolder
    {
        private List<IMappedObject> children = new List<IMappedObject>();

        public List<IMappedObject> Children { get { return children; } set { this.children = value; } }

        public MappedFolder(MappedObjectData data, IMetaDataStorage storage, IFileSystemInfoFactory fsFactory = null)
            : base(data, storage, fsFactory)
        {
            this.Type = MappedObjectType.Folder;
        }
    }
}
