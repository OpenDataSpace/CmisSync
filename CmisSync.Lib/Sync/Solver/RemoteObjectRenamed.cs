using System;
using System.IO;

using DotCMIS.Client;

using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;

namespace CmisSync.Lib.Sync.Solver
{
    public class RemoteObjectRenamed : ISolver
    {
        public virtual void Solve(ISession session, IMetaDataStorage storage, FileSystemInfo localFile, string remoteId){
            // Rename local object
            throw new NotImplementedException();
        }
    }
}

