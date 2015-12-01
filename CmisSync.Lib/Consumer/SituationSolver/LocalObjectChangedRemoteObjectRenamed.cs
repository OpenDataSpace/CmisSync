//-----------------------------------------------------------------------
// <copyright file="LocalObjectChangedRemoteObjectRenamed.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Consumer.SituationSolver {
    using System;
    using System.IO;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    /// <summary>
    /// Local object changed remote object renamed solver.
    /// </summary>
    public class LocalObjectChangedRemoteObjectRenamed : AbstractEnhancedSolver {
        private readonly ISolver changeChangeSolver;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="CmisSync.Lib.Consumer.SituationSolver.LocalObjectChangedRemoteObjectRenamed"/> class.
        /// </summary>
        /// <param name="session">Cmis session.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="changeChangeSolver">Local change remote change solver.</param>
        public LocalObjectChangedRemoteObjectRenamed(
            ISession session,
            IMetaDataStorage storage,
            ISolver changeChangeSolver) : base(session, storage)
        {
            if (changeChangeSolver == null) {
                throw new ArgumentNullException("changeChangeSolver", "Given solver for the situation of local and remote changes is null");
            }

            this.changeChangeSolver = changeChangeSolver;
        }

        public override void Solve(
            IFileSystemInfo localFileSystemInfo,
            IObjectId remoteId,
            ContentChangeType localContent,
            ContentChangeType remoteContent)
        {
            if (remoteId == null) {
                throw new ArgumentNullException("remoteId");
            }

            if (localFileSystemInfo == null) {
                throw new ArgumentNullException("localFileSystemInfo");
            }

            // Rename local object and call change/change solver
            var savedObject = this.Storage.GetObjectByRemoteId(remoteId.Id);
            string oldPath = localFileSystemInfo.FullName;
            string parentPath = localFileSystemInfo is IFileInfo ? (localFileSystemInfo as IFileInfo).Directory.FullName : (localFileSystemInfo as IDirectoryInfo).Parent.FullName;
            string newPath = Path.Combine(parentPath, (remoteId as ICmisObject).Name);
            this.MoveTo(localFileSystemInfo, oldPath, newPath);
            savedObject.Name = (remoteId as ICmisObject).Name;
            savedObject.Ignored = (remoteId as ICmisObject).AreAllChildrenIgnored();
            this.Storage.SaveMappedObject(savedObject);
            this.changeChangeSolver.Solve(localFileSystemInfo, remoteId, localContent, remoteContent);
        }

        private void MoveTo(IFileSystemInfo localFsInfo, string oldPath, string newPath) {
            if (localFsInfo is IFileInfo) {
                (localFsInfo as IFileInfo).MoveTo(newPath);
                OperationsLogger.Info(string.Format("Moved local file {0} to {1}", oldPath, newPath));
            } else {
                (localFsInfo as IDirectoryInfo).MoveTo(newPath);
                OperationsLogger.Info(string.Format("Moved local folder {0} to {1}", oldPath, newPath));
            }
        }
    }
}