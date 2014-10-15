//-----------------------------------------------------------------------
// <copyright file="LocalObjectMovedRemoteObjectMoved.cs" company="GRAU DATA AG">
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
    using System.IO;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    /// <summary>
    /// Local object moved and remote object moved.
    /// </summary>
    public class LocalObjectMovedRemoteObjectMoved : AbstractEnhancedSolver
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="CmisSync.Lib.Consumer.SituationSolver.LocalObjectMovedRemoteObjectMoved"/> class.
        /// </summary>
        /// <param name="session">Cmis Session.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="isServerAbleToUpdateModificationDate">If set to <c>true</c> the server is able to update modification date.</param>
        public LocalObjectMovedRemoteObjectMoved(
            ISession session,
            IMetaDataStorage storage) : base(
            session,
            storage) {
        }

        /// <summary>
        /// Solve the specified situation by using localFile and remote object.
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
            if (localFileSystemInfo is IDirectoryInfo) {
                IDirectoryInfo localFolder = localFileSystemInfo as IDirectoryInfo;
                IDirectoryInfo localParent = localFolder.Parent;
                IFolder remoteFolder = remoteId as IFolder;
                string remoteParentId = remoteFolder.ParentId;
                IMappedObject mappedParent = this.Storage.GetObjectByRemoteId(remoteParentId);
                IMappedObject mappedObject = this.Storage.GetObjectByRemoteId(remoteFolder.Id);
                if (localParent.Uuid == mappedParent.Guid) {
                    // Both folders are in the same parent folder
                    this.SyncNamesAndDates(localFolder, remoteFolder, mappedObject);
                } else {
                    OperationsLogger.Warn(
                        string.Format(
                        "Synchronization Conflict: The local directory {0} has been moved to {1} with id {2},{4}" +
                        "but the remote folder was moved to {3}{4}You can fix this situation by moving them into the same folder",
                        localFileSystemInfo.Name,
                        localFileSystemInfo.FullName,
                        remoteFolder.Path,
                        Environment.NewLine));
                    return;
                }
            } else if (localFileSystemInfo is IFileInfo) {
                IFileInfo localFile = localFileSystemInfo as IFileInfo;
                IDirectoryInfo localParent = localFile.Directory;
                IDocument remoteFile = remoteId as IDocument;
                string remoteParentId = remoteFile.Parents[0].Id;
                IMappedObject mappedParent = this.Storage.GetObjectByRemoteId(remoteParentId);
                IMappedObject mappedObject = this.Storage.GetObjectByRemoteId(remoteFile.Id);
                if (localParent.Uuid == mappedParent.Guid) {
                    // Both files are in the same parent folder
                    this.SyncNamesAndDates(localFile, remoteFile, mappedObject);
                    return;
                } else {
                    OperationsLogger.Warn(
                        string.Format(
                        "Synchronization Conflict: The local file {0} has been moved to {1} with id {2},{4}" +
                        "but the remote file was moved to {3}{4}You can fix this situation by moving them into the same folder",
                        localFileSystemInfo.Name,
                        localFileSystemInfo.FullName,
                        remoteFile.Paths[0],
                        Environment.NewLine));
                    return;
                }
            }
        }

        private void SyncNamesAndDates(IFileSystemInfo local, IFileableCmisObject remote, IMappedObject mappedObject)
        {
            DateTime? oldRemoteModificationDate = remote.LastModificationDate;
            DateTime oldLocalModificationDate = local.LastWriteTimeUtc;

            // Sync Names
            if (mappedObject.Name != local.Name && mappedObject.Name == remote.Name) {
                // local has been renamed => rename remote
                remote.Rename(local.Name, true);
                mappedObject.Name = local.Name;
            } else if (mappedObject.Name == local.Name && mappedObject.Name != remote.Name) {
                // remote has been renamed => rename local
                if (local is IFileInfo)
                {
                    IFileInfo localFile = local as IFileInfo;
                    localFile.MoveTo(Path.Combine(localFile.Directory.FullName, remote.Name));
                }
                else if (local is IDirectoryInfo)
                {
                    IDirectoryInfo localFolder = local as IDirectoryInfo;
                    localFolder.MoveTo(Path.Combine(localFolder.Parent.FullName, remote.Name));
                }
                else
                {
                    throw new ArgumentException("Solved move conflict => invoke crawl sync to detect other changes");
                }
                mappedObject.Name = remote.Name;
            } else if (mappedObject.Name != local.Name && mappedObject.Name != remote.Name) {
                // both are renamed => rename to the latest change
                DateTime localModification = local.LastWriteTimeUtc;
                DateTime remoteModification = (DateTime)remote.LastModificationDate;
                if (localModification > remoteModification) {
                    // local modification is newer
                    remote.Rename(local.Name, true);
                    mappedObject.Name = local.Name;
                } else {
                    // remote modification is newer
                    if (local is IFileInfo)
                    {
                        IFileInfo localFile = local as IFileInfo;
                        localFile.MoveTo(Path.Combine(localFile.Directory.FullName, remote.Name));
                    }
                    else if (local is IDirectoryInfo)
                    {
                        IDirectoryInfo localFolder = local as IDirectoryInfo;
                        localFolder.MoveTo(Path.Combine(localFolder.Parent.FullName, remote.Name));
                    }
                    else
                    {
                        throw new ArgumentException("Solved move conflict => invoke crawl sync to detect other changes");
                    }
                    local.LastWriteTimeUtc = (DateTime)remote.LastModificationDate;
                    mappedObject.Name = remote.Name;
                }
            }

            // Sync modification dates
            if (oldRemoteModificationDate != null) {
                if (oldLocalModificationDate > oldRemoteModificationDate && this.ServerCanModifyDateTimes) {
                    remote.UpdateLastWriteTimeUtc(oldLocalModificationDate);
                    local.LastWriteTimeUtc = oldLocalModificationDate;
                } else if (oldLocalModificationDate < (DateTime)oldRemoteModificationDate) {
                    local.LastWriteTimeUtc = (DateTime)oldRemoteModificationDate;
                }
            }

            mappedObject.LastLocalWriteTimeUtc = local.LastWriteTimeUtc;
            mappedObject.LastRemoteWriteTimeUtc = (DateTime)remote.LastModificationDate;
            mappedObject.ParentId = remote.Parents[0].Id;
            mappedObject.LastChangeToken = remote.ChangeToken;
            this.Storage.SaveMappedObject(mappedObject);
        }
    }
}