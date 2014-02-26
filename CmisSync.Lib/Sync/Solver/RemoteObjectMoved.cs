using System;
using System.IO;

using DotCMIS.Client;

using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;

namespace CmisSync.Lib.Sync.Solver
{
    public class RemoteObjectMoved : ISolver
    {
        public virtual void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFile, IObjectId remoteId){
            // Move local object
            throw new NotImplementedException();
        }
    }
}

