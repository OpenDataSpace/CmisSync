//-----------------------------------------------------------------------
// <copyright file="LocalObjectChangedRemoteObjectChangedWithPWC.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------

namespace CmisSync.Lib.Consumer.SituationSolver.PWC {
    using System;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    /// <summary>
    /// Local object changed remote object changed situations are decorated if only the local content has been changed. Otherwise the given fallback will solve the situation.
    /// </summary>
    public class LocalObjectChangedRemoteObjectChangedWithPWC : AbstractEnhancedSolverWithPWC {
        private readonly ISolver fallbackSolver;
        private readonly ActiveActivitiesManager transmissionManager;

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
            if (localObjectChangedRemoteObjectChangedFallbackSolver == null) {
                throw new ArgumentNullException("Given fallback solver is null");
            }

            if (!session.ArePrivateWorkingCopySupported()) {
                throw new ArgumentException("Given session does not support pwc updates");
            }

            this.fallbackSolver = localObjectChangedRemoteObjectChangedFallbackSolver;
            this.transmissionManager = manager;
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
            if (localFileSystemInfo is IFileInfo && remoteContent == ContentChangeType.NONE) {
                this.fallbackSolver.Solve(localFileSystemInfo, remoteId, localContent, remoteContent);
            } else {
                this.fallbackSolver.Solve(localFileSystemInfo, remoteId, localContent, remoteContent);
            }
        }
    }
}