using System;
using System.IO;

using DotCMIS.Client;

using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;

namespace CmisSync.Lib.Sync.Solver
{
    public class LocalObjectDeleted : ISolver
    {
        public virtual void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFile, IObjectId remoteId){
            session.Delete(remoteId, true);
            var mappedObject = storage.GetObjectByRemoteId(remoteId.Id);
            mappedObject.Remove();
        }
    }
}

