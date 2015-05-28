//-----------------------------------------------------------------------
// <copyright file="LocalObjectMovedRemoteObjectRenamed.cs" company="GRAU DATA AG">
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
    using DotCMIS.Exceptions;

    public class LocalObjectMovedRemoteObjectRenamed : AbstractEnhancedSolver {
        private readonly ISolver changeChangeSolver;
        private readonly ISolver renameRenameSolver;
        public LocalObjectMovedRemoteObjectRenamed(
            ISession session,
            IMetaDataStorage storage,
            ISolver changeChangeSolver,
            ISolver renameRenameSolver) : base(session, storage)
        {
            if (changeChangeSolver == null) {
                throw new ArgumentNullException("Given solver for local and remote change situation is null");
            }

            if (renameRenameSolver == null) {
                throw new ArgumentNullException("Given solver for local and remote rename situation is null");
            }

            this.changeChangeSolver = changeChangeSolver;
            this.renameRenameSolver = renameRenameSolver;
        }

        public override void Solve(
            IFileSystemInfo localFileSystemInfo,
            IObjectId remoteId,
            ContentChangeType localContent,
            ContentChangeType remoteContent)
        {
            var savedObject = this.Storage.GetObjectByRemoteId(remoteId.Id);
            Guid? newParentUuid = localFileSystemInfo is IFileInfo ? (localFileSystemInfo as IFileInfo).Directory.Uuid : (localFileSystemInfo as IDirectoryInfo).Parent.Uuid;
            string newParentId = this.Storage.GetObjectByGuid((Guid)newParentUuid).RemoteObjectId;
            savedObject.Ignored = (remoteId as ICmisObject).AreAllChildrenIgnored();
            if (localFileSystemInfo.Name == (remoteId as ICmisObject).Name) {
                // Both names are equal => only move to new remote parent
                try {
                    (remoteId as IFileableCmisObject).Move(this.Session.GetObject(savedObject.ParentId), this.Session.GetObject(newParentId));
                } catch (CmisPermissionDeniedException) {
                    OperationsLogger.Info(string.Format("Permission Denied: Cannot move remote object {0} from {1} to {2}", remoteId.Id, savedObject.ParentId, newParentId));
                    return;
                }

                savedObject.Name = localFileSystemInfo.Name;
                savedObject.ParentId = newParentId;
                this.Storage.SaveMappedObject(savedObject);
                this.changeChangeSolver.Solve(localFileSystemInfo, remoteId, localContent, remoteContent);
            } else {
                // Names are different to each other
                if (localFileSystemInfo.Name == savedObject.Name) {
                    // Remote rename and local move => Move remote and rename locally => change change solver
                    try {
                        remoteId = (remoteId as IFileableCmisObject).Move(this.Session.GetObject(savedObject.ParentId), this.Session.GetObject(newParentId));
                    } catch (CmisPermissionDeniedException) {
                        OperationsLogger.Info(string.Format("Permission Denied: Cannot move remote object {0} from {1} to {2}", remoteId.Id, savedObject.ParentId, newParentId));
                        return;
                    }

                    var localParentPath = localFileSystemInfo is IFileInfo ? (localFileSystemInfo as IFileInfo).Directory.FullName : (localFileSystemInfo as IDirectoryInfo).Parent.FullName;
                    string newPath = Path.Combine(localParentPath, (remoteId as ICmisObject).Name);
                    this.MoveTo(localFileSystemInfo, localFileSystemInfo.FullName, newPath);
                    savedObject.Name = localFileSystemInfo.Name;
                    savedObject.ParentId = newParentId;
                    this.Storage.SaveMappedObject(savedObject);
                    this.changeChangeSolver.Solve(localFileSystemInfo, remoteId, localContent, remoteContent);
                } else {
                    // Both sides have been renamed => Move remote => rename rename solver
                    try {
                        (remoteId as IFileableCmisObject).Move(this.Session.GetObject(savedObject.ParentId), this.Session.GetObject(newParentId));
                    } catch (CmisPermissionDeniedException) {
                        OperationsLogger.Info(string.Format("Permission Denied: Cannot move remote object {0} from {1} to {2}", remoteId.Id, savedObject.ParentId, newParentId));
                        return;
                    }

                    savedObject.ParentId = newParentId;
                    this.Storage.SaveMappedObject(savedObject);
                    this.renameRenameSolver.Solve(localFileSystemInfo, remoteId, localContent, remoteContent);
                }
            }
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