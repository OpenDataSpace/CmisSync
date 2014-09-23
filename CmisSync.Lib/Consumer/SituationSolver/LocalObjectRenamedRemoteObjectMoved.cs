//-----------------------------------------------------------------------
// <copyright file="LocalObjectRenamedRemoteObjectMoved.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Consumer.SituationSolver
{
    using System;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    public class LocalObjectRenamedRemoteObjectMoved : AbstractEnhancedSolver
    {
        private LocalObjectRenamedRemoteObjectRenamed renameRenameSolver;
        private LocalObjectChangedRemoteObjectChanged changeChangeSolver;
        public LocalObjectRenamedRemoteObjectMoved(
            ISession session,
            IMetaDataStorage storage,
            LocalObjectRenamedRemoteObjectRenamed renameSolver,
            LocalObjectChangedRemoteObjectChanged changeSolver) : base(session, storage)
        {
            if (renameSolver == null) {
                throw new ArgumentNullException("Given solver for rename rename situation is null");
            }

            if (changeSolver == null) {
                throw new ArgumentNullException("Given solver for change change situation is null");
            }

            this.changeChangeSolver = changeSolver;
            this.renameRenameSolver = renameSolver;
        }

        public override void Solve(
            IFileSystemInfo localFileSystemInfo,
            IObjectId remoteId,
            ContentChangeType localContent,
            ContentChangeType remoteContent)
        {
            var savedObject = this.Storage.GetObjectByRemoteId(remoteId.Id);
            if ((remoteId as ICmisObject).Name != savedObject.Name) {
                // Both are renamed and remote is also moved => move & rename/rename
                throw new NotImplementedException();
                // Move local object
                // this.renameRenameSolver.Solve(localFileSystemInfo, remoteId, localContent, remoteContent);
            } else {
                // Local rename and remote move => move locally and rename remote => change/change
                try {
                    // rename remote file
                    string oldName = (remoteId as ICmisObject).Name;
                    (remoteId as ICmisObject).Rename(localFileSystemInfo.Name, true);
                    OperationsLogger.Info(string.Format("Renamed remote object {0} from {1} to {2}", remoteId.Id, oldName, localFileSystemInfo.Name));
                    savedObject.Name = (remoteId as ICmisObject).Name;
                } catch (CmisPermissionDeniedException) {
                    OperationsLogger.Info(string.Format("Permission Denied: Cannot rename remote object ({1}): {0}", (remoteId as ICmisObject).Name, remoteId.Id));
                    return;
                }
                string oldPath = localFileSystemInfo.FullName;
                string newPath = remoteId is IFolder ? this.Storage.Matcher.CreateLocalPath(remoteId as IFolder) : this.Storage.Matcher.CreateLocalPath(remoteId as IDocument);
                // move local object to same directory as the remote object is
                if (localFileSystemInfo is IFileInfo) {
                    (localFileSystemInfo as IFileInfo).MoveTo(newPath);
                    OperationsLogger.Info(string.Format("Moved local file {0} to {1}", oldPath, newPath));
                } else {
                    (localFileSystemInfo as IDirectoryInfo).MoveTo(newPath);
                    OperationsLogger.Info(string.Format("Moved local folder {0} to {1}", oldPath, newPath));
                }

                savedObject.ParentId = remoteId is IFolder ? (remoteId as IFolder).ParentId : (remoteId as IDocument).Parents[0].Id;
                this.Storage.SaveMappedObject(savedObject);

                // Synchronize the rest with the change change solver
                this.changeChangeSolver.Solve(localFileSystemInfo, remoteId, localContent, remoteContent);
            }
        }
    }
}