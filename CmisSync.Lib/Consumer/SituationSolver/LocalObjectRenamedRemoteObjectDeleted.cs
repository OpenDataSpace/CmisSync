using CmisSync.Lib.Queueing;


namespace CmisSync.Lib.Consumer.SituationSolver
{
    using System;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    public class LocalObjectRenamedRemoteObjectDeleted : AbstractEnhancedSolver
    {
        private ISolver secondSolver;
        public LocalObjectRenamedRemoteObjectDeleted(
            ISession session,
            IMetaDataStorage storage,
            ISyncEventQueue queue,
            ActiveActivitiesManager manager,
            bool serverCanModifyDates,
            ISolver secondSolver = null) : base(session, storage, serverCanModifyDates) {
            this.secondSolver = secondSolver ?? new LocalObjectAdded(session, storage, queue, manager, serverCanModifyDates);
        }

        public override void Solve(
            IFileSystemInfo localFileSystemInfo,
            IObjectId remoteId,
            ContentChangeType localContent,
            ContentChangeType remoteContent)
        {
            throw new NotImplementedException();
        }
    }
}