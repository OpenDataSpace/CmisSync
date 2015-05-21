//-----------------------------------------------------------------------
// <copyright file="LocalObjectChangedRemoteObjectMoved.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    /// <summary>
    /// Local object changed remote object moved situation solver.
    /// </summary>
    public class LocalObjectChangedRemoteObjectMoved : AbstractEnhancedSolver {
        private readonly ISolver changeChangeSolver;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="CmisSync.Lib.Consumer.SituationSolver.LocalObjectChangedRemoteObjectMoved"/> class.
        /// </summary>
        /// <param name="session">Cmis session.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="changeSolver">Local object changed and remote object changed situation solver.</param>
        public LocalObjectChangedRemoteObjectMoved(
            ISession session,
            IMetaDataStorage storage,
            ISolver changeChangeSolver) : base(session, storage)
        {
            if (changeChangeSolver == null) {
                throw new ArgumentNullException("changeChangeSolver", "Given solver for the conflict situation of local and remote change is null");
            }

            this.changeChangeSolver = changeChangeSolver;
        }

        public override void Solve(
            IFileSystemInfo localFileSystemInfo,
            IObjectId remoteId,
            ContentChangeType localContent,
            ContentChangeType remoteContent)
        {
            var savedObject = this.Storage.GetObjectByRemoteId(remoteId.Id);
            string newPath = remoteId is IFolder ? this.Storage.Matcher.CreateLocalPath(remoteId as IFolder) : this.Storage.Matcher.CreateLocalPath(remoteId as IDocument);
            if (remoteId is IFolder) {
                IDirectoryInfo dirInfo = localFileSystemInfo as IDirectoryInfo;
                string oldPath = dirInfo.FullName;
                if (!dirInfo.FullName.Equals(newPath)) {
                    dirInfo.MoveTo(newPath);
                    OperationsLogger.Info(string.Format("Moved local folder {0} to {1}", oldPath, newPath));
                } else {
                    return;
                }
            } else if (remoteId is IDocument) {
                IFileInfo fileInfo = localFileSystemInfo as IFileInfo;
                string oldPath = fileInfo.FullName;
                fileInfo.MoveTo(newPath);
                OperationsLogger.Info(string.Format("Moved local file {0} to {1}", oldPath, newPath));
            }

            savedObject.Name = (remoteId as ICmisObject).Name;
            savedObject.Ignored = (remoteId as ICmisObject).AreAllChildrenIgnored();
            savedObject.ParentId = remoteId is IFolder ? (remoteId as IFolder).ParentId : (remoteId as IDocument).Parents[0].Id;
            this.Storage.SaveMappedObject(savedObject);

            this.changeChangeSolver.Solve(localFileSystemInfo, remoteId, localContent, remoteContent);
        }
    }
}