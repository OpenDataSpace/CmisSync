using System;
using System.IO;

using DotCMIS.Client;

using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;

namespace CmisSync.Lib.Sync.Solver
{
    public class LocalObjectMoved : ISolver
    {
        public virtual void Solve(ISession session, MetaDataStorage storage, FileSystemInfo localFile, string remoteId){
            // Move Remote Object
            throw new NotImplementedException();
        }
    }
}

