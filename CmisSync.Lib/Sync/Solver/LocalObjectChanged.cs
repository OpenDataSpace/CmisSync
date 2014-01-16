using System;
using System.IO;

using DotCMIS.Client;

using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;

namespace CmisSync.Lib.Sync.Solver
{
    public class LocalObjectChanged : AbstractSolver
    {
        public LocalObjectChanged(ISyncEventQueue queue) : base (queue) {}

        public override void Solve(ISession session, MetaDataStorage storage, FileSystemInfo localFile, string remoteId){
            // Match local changes to remote changes and updated them remotely
            throw new NotImplementedException();
        }
    }
}
