using System;

using System.IO;

using DotCMIS.Client;

using CmisSync.Lib.Data;
using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;

namespace CmisSync.Lib.Sync.Solver
{
    using log4net;

    public class RemoteObjectAdded : ISolver
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RemoteObjectAdded));

        public virtual void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFile, IObjectId remoteId){

            if(localFile is IDirectoryInfo) {
                if(!(remoteId is IFolder)) {
                    throw new ArgumentException("remoteId has to be a prefetched Folder");
                }
                var remoteFolder = remoteId as IFolder;
                (localFile as IDirectoryInfo).Create();
                var mappedObject = new MappedObject(remoteFolder);
                storage.SaveMappedObject(mappedObject);
            }

        }
    }
}

