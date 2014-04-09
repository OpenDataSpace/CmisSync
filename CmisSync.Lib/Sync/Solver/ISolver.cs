using System;
using System.IO;
using DotCMIS.Client;

using CmisSync.Lib.Events;
using CmisSync.Lib.Data;
using CmisSync.Lib.Storage;

namespace CmisSync.Lib.Sync.Solver
{
    public interface ISolver 
    {
        void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFile, IObjectId remoteId);
    }
}

