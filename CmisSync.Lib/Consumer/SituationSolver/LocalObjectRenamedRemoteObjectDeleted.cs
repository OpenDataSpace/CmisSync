namespace CmisSync.Lib.Consumer.SituationSolver
{
    using System;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
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
            var mappedObject = this.Storage.GetObjectByGuid((Guid)localFileSystemInfo.Uuid);
            this.Storage.RemoveObject(mappedObject);
            if (localFileSystemInfo is IFileInfo) {
                this.secondSolver.Solve(localFileSystemInfo, null, ContentChangeType.CREATED, ContentChangeType.NONE);
            } else if (localFileSystemInfo is IDirectoryInfo) {
                this.secondSolver.Solve(localFileSystemInfo, null, ContentChangeType.NONE, ContentChangeType.NONE);
                var dir = localFileSystemInfo as IDirectoryInfo;
                if (dir.GetFiles().Length > 0 || dir.GetDirectories().Length > 0) {
                    throw new IOException(string.Format("There are unsynced files in local folder {0} => starting crawl sync", dir.FullName));
                }
            }
        }
    }
}