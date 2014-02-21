using System;
using System.IO;

using CmisSync.Lib.Storage;

using DotCMIS.Client;

namespace CmisSync.Lib.Sync.Solver
{
    public class NothingToDoSolver : ISolver
    {
        public virtual void Solve (ISession session, IMetaDataStorage storage, IFileSystemInfo localFile, string remoteId)
        {
            // No Operation Needed
        }
    }
}

