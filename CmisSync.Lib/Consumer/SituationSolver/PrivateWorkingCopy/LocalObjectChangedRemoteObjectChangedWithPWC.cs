
namespace CmisSync.Lib.Consumer.SituationSolver.PWC {
    using System;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    /// <summary>
    /// Local object changed remote object changed situations are decorated if only the local content has been changed. Otherwise the given fallback will solve the situation.
    /// </summary>
    public class LocalObjectChangedRemoteObjectChangedWithPWC : AbstractEnhancedSolverWithPWC {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="CmisSync.Lib.Consumer.SituationSolver.PWC.LocalObjectChangedRemoteObjectChangedWithPWC"/> class.
        /// </summary>
        /// <param name="session">Cmis session.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="transmissionStorage">Transmission storage.</param>
        /// <param name="manager">Transmission manager.</param>
        /// <param name="localObjectChangedRemoteObjectChangedFallbackSolver">Local object changed remote object changed fallback solver.</param>
        public LocalObjectChangedRemoteObjectChangedWithPWC(
            ISession session,
            IMetaDataStorage storage,
            IFileTransmissionStorage transmissionStorage,
            ActiveActivitiesManager manager,
            ISolver localObjectChangedRemoteObjectChangedFallbackSolver) : base(session, storage, transmissionStorage)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Solve the specified situation by using localFile and remote object if the local file content is the only changed content. Otherwise the given fallback will be used.
        /// </summary>
        /// <param name="localFileSystemInfo">Local filesystem info instance.</param>
        /// <param name="remoteId">Remote identifier or object.</param>
        /// <param name="localContent">Hint if the local content has been changed.</param>
        /// <param name="remoteContent">Information if the remote content has been changed.</param>
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