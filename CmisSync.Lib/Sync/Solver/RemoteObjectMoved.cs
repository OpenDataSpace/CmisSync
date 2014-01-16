using System;
using System.IO;

using DotCMIS.Client;

using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;

namespace CmisSync.Lib.Sync.Solver
{
    public class RemoteObjectMoved : AbstractSolver
    {

        public RemoteObjectMoved(ISyncEventQueue queue) : base (queue) {}

        public override void Solve(ISession session, MetaDataStorage storage, FileSystemInfo localFile, string remoteId){
            // Move local object
            throw new NotImplementedException();
        }
    }
}
