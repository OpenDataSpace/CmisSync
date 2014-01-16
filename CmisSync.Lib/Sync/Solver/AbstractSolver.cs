using System;
using System.IO;

using DotCMIS.Client;

using CmisSync.Lib.Storage;
using CmisSync.Lib.Events;


namespace CmisSync.Lib.Sync.Solver
{
    public abstract class AbstractSolver : AbstractEventProducer, ISolver
    {
        public AbstractSolver(ISyncEventQueue queue) : base(queue) { }

        public abstract void Solve(ISession session, MetaDataStorage storage, FileSystemInfo localFile, string remoteId);
    }
}

