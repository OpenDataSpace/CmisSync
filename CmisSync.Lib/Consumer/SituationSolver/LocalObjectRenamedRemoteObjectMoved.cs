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

namespace CmisSync.Lib.Consumer.SituationSolver {
    using System;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Exceptions;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    /// <summary>
    /// Local object renamed remote object moved solver.
    /// </summary>
    public class LocalObjectRenamedRemoteObjectMoved : AbstractEnhancedSolver {
        private readonly ISolver renameRenameSolver;
        private readonly ISolver changeChangeSolver;
        public LocalObjectRenamedRemoteObjectMoved(
            ISession session,
            IMetaDataStorage storage,
            ISolver renameSolver,
            ISolver changeChangeSolver) : base(session, storage)
        {
            if (renameSolver == null) {
                throw new ArgumentNullException("renameSolver", "Given solver for rename rename situation is null");
            }

            if (changeChangeSolver == null) {
                throw new ArgumentNullException("changeChangeSolver", "Given solver for change change situation is null");
            }

            this.changeChangeSolver = changeChangeSolver;
            this.renameRenameSolver = renameSolver;
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

            var savedObject = this.Storage.GetObjectByRemoteId(remoteId.Id);
            string oldPath = localFileSystemInfo.FullName;
            string oldName = (remoteId as ICmisObject).Name;
            if ((remoteId as ICmisObject).Name != savedObject.Name) {
                // Both are renamed and remote is also moved => move & rename/rename
                string newPath = remoteId is IFolder ? this.Storage.Matcher.CreateLocalPath(remoteId as IFolder) : this.Storage.Matcher.CreateLocalPath(remoteId as IDocument);
                if ((remoteId as ICmisObject).Name == localFileSystemInfo.Name) {
                    // Move local object to new name, bacause it is the same => only change/change solver is needed
                    this.MoveTo(localFileSystemInfo, oldPath, newPath);
                    savedObject.Name = localFileSystemInfo.Name;
                    savedObject.ParentId = remoteId is IFolder ? (remoteId as IFolder).ParentId : (remoteId as IDocument).Parents[0].Id;
                    this.Storage.SaveMappedObject(savedObject);
                    this.changeChangeSolver.Solve(localFileSystemInfo, remoteId, localContent, remoteContent);
                } else {
                    // Only move local object to new folder but keep the old name => both names are different => rename/rename solver needed
                    newPath = newPath.TrimEnd(Path.DirectorySeparatorChar);
                    newPath = newPath.Substring(0, newPath.Length - (remoteId as ICmisObject).Name.Length) + oldName;
                    this.MoveTo(localFileSystemInfo, oldPath, newPath);
                    savedObject.ParentId = remoteId is IFolder ? (remoteId as IFolder).ParentId : (remoteId as IDocument).Parents[0].Id;
                    this.Storage.SaveMappedObject(savedObject);
                    this.renameRenameSolver.Solve(localFileSystemInfo, remoteId, localContent, remoteContent);
                }
            } else {
                // Local rename and remote move => move locally and rename remote => change/change
                try {
                    // rename remote file
                    (remoteId as ICmisObject).Rename(localFileSystemInfo.Name, true);
                    OperationsLogger.Info(string.Format("Renamed remote object {0} from {1} to {2}", remoteId.Id, oldName, localFileSystemInfo.Name));
                    savedObject.Name = (remoteId as ICmisObject).Name;
                } catch (CmisConstraintException e) {
                    if (!Utils.IsValidISO885915(localFileSystemInfo.Name)) {
                        OperationsLogger.Warn(string.Format("Server denied to rename {0} to {1}, perhaps because it contains UTF-8 characters", oldName, localFileSystemInfo.Name));
                        throw new InteractionNeededException(string.Format("Server denied renaming of {0}", oldName), e) {
                            Title = string.Format("Server denied renaming of {0}", oldName),
                            Description = string.Format("Server denied to rename {0} to {1}, perhaps because it contains UTF-8 characters", oldName, localFileSystemInfo.Name)
                        };
                    }

                    throw;
                } catch (CmisPermissionDeniedException) {
                    OperationsLogger.Info(string.Format("Permission Denied: Cannot rename remote object ({1}): {0}", (remoteId as ICmisObject).Name, remoteId.Id));
                    return;
                }

                string newPath = remoteId is IFolder ? this.Storage.Matcher.CreateLocalPath(remoteId as IFolder) : this.Storage.Matcher.CreateLocalPath(remoteId as IDocument);
                // move local object to same directory as the remote object is
                this.MoveTo(localFileSystemInfo, oldPath, newPath);

                savedObject.ParentId = remoteId is IFolder ? (remoteId as IFolder).ParentId : (remoteId as IDocument).Parents[0].Id;
                this.Storage.SaveMappedObject(savedObject);

                // Synchronize the rest with the change change solver
                this.changeChangeSolver.Solve(localFileSystemInfo, remoteId, localContent, remoteContent);
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