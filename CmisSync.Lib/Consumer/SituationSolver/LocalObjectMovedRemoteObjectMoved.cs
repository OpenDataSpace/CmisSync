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
            IMetaDataStorage storage,
            bool isServerAbleToUpdateModificationDate) : base(
            session,
            storage,
            isServerAbleToUpdateModificationDate) {
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
                IMappedObject mappedParent = this.Storage.GetObjectByRemoteId(remoteFolder.ParentId);
                IMappedObject mappedObject = this.Storage.GetObjectByRemoteId(remoteFolder.Id);
                if (localParent.Uuid == mappedParent.Guid) {
                    // Both folders are in the same parent folder
                    this.SyncNamesAndDates(localFolder, remoteFolder, mappedObject);
                } else {
                    throw new NotImplementedException();
                }
            } else {
                throw new NotImplementedException();
            }
        }

        private void SyncNamesAndDates(IDirectoryInfo localFolder, IFolder remoteFolder, IMappedObject mappedObject)
        {
            DateTime? oldRemoteModificationDate = remoteFolder.LastModificationDate;
            DateTime oldLocalModificationDate = localFolder.LastWriteTimeUtc;

            // Sync Names
            if (mappedObject.Name != localFolder.Name && mappedObject.Name == remoteFolder.Name) {
                // local folder has been renamed => rename remote folder
                remoteFolder.Rename(localFolder.Name, true);
                mappedObject.Name = localFolder.Name;
            } else if (mappedObject.Name == localFolder.Name && mappedObject.Name != remoteFolder.Name) {
                // remote folder has been renamed => rename local folder
                localFolder.MoveTo(Path.Combine(localFolder.Parent.FullName, remoteFolder.Name));
                mappedObject.Name = remoteFolder.Name;
            } else if (mappedObject.Name != localFolder.Name && mappedObject.Name != remoteFolder.Name) {
                // both folders are renamed => rename to the latest change
                DateTime localModification = localFolder.LastWriteTimeUtc;
                DateTime remoteModification = (DateTime)remoteFolder.LastModificationDate;
                if (localModification > remoteModification) {
                    // local modification is newer
                    remoteFolder.Rename(localFolder.Name, true);
                    mappedObject.Name = localFolder.Name;
                } else {
                    // remote modification is newer
                    localFolder.MoveTo(Path.Combine(localFolder.Parent.FullName, remoteFolder.Name));
                    localFolder.LastWriteTimeUtc = (DateTime)remoteFolder.LastModificationDate;
                    mappedObject.Name = remoteFolder.Name;
                }
            }

            // Sync modification dates
            if (oldRemoteModificationDate != null) {
                if (oldLocalModificationDate > oldRemoteModificationDate && this.ServerCanModifyDateTimes) {
                    remoteFolder.UpdateLastWriteTimeUtc(oldLocalModificationDate);
                    localFolder.LastWriteTimeUtc = oldLocalModificationDate;
                } else if (oldLocalModificationDate < (DateTime)oldRemoteModificationDate) {
                    localFolder.LastWriteTimeUtc = (DateTime)oldRemoteModificationDate;
                }
            }

            mappedObject.LastLocalWriteTimeUtc = localFolder.LastWriteTimeUtc;
            mappedObject.LastRemoteWriteTimeUtc = (DateTime)remoteFolder.LastModificationDate;
            mappedObject.ParentId = remoteFolder.ParentId;
            mappedObject.LastChangeToken = remoteFolder.ChangeToken;
            this.Storage.SaveMappedObject(mappedObject);
        }
    }
}