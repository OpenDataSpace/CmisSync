//-----------------------------------------------------------------------
// <copyright file="LocalObjectRenamedRemoteObjectRenamed.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;
    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    /// <summary>
    /// Local object renamed and also the remote object has been renamed.
    /// </summary>
    public class LocalObjectRenamedRemoteObjectRenamed : AbstractEnhancedSolver
    {
        private LocalObjectChangedRemoteObjectChanged changeChangeSolver;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="CmisSync.Lib.Consumer.SituationSolver.LocalObjectRenamedRemoteObjectRenamed"/> class.
        /// </summary>
        /// <param name="session">Cmis session.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="serverCanModifyCreationAndModificationDate">If set to <c>true</c> server can modify creation and modification date.</param>
        public LocalObjectRenamedRemoteObjectRenamed(
            ISession session,
            IMetaDataStorage storage,
            LocalObjectChangedRemoteObjectChanged changeSolver) : base(session, storage) {
            if (changeSolver == null) {
                throw new ArgumentNullException("Given solver for the situation of local and remote object changed is null");
            }

            this.changeChangeSolver = changeSolver;
        }

        /// <summary>
        /// Solve the specified situation by taking renaming the local or remote object to the name of the last changed object.
        /// </summary>
        /// <param name="localFileSystemInfo">Local file system info.</param>
        /// <param name="remoteId">Remote object.</param>
        /// <param name="localContent">Hint if the local content has been changed.</param>
        /// <param name="remoteContent">Information if the remote content has been changed.</param>
        public override void Solve(
            IFileSystemInfo localFileSystemInfo,
            IObjectId remoteId,
            ContentChangeType localContent = ContentChangeType.NONE,
            ContentChangeType remoteContent = ContentChangeType.NONE)
        {
            if (localFileSystemInfo is IDirectoryInfo) {
                var localFolder = localFileSystemInfo as IDirectoryInfo;
                var remoteFolder = remoteId as IFolder;
                var mappedObject = this.Storage.GetObjectByRemoteId(remoteFolder.Id);
                if (localFolder.Name.Equals(remoteFolder.Name)) {
                    mappedObject.Name = localFolder.Name;
                } else if (localFolder.LastWriteTimeUtc.CompareTo((DateTime)remoteFolder.LastModificationDate) > 0) {
                    string oldName = remoteFolder.Name;
                    try {
                        remoteFolder.Rename(localFolder.Name, true);
                    } catch (CmisConstraintException e) {
                        if (!Utils.IsValidISO885915(localFolder.Name)) {
                            OperationsLogger.Warn(string.Format("Server denied to rename {0} to {1}, perhaps because it contains UTF-8 characters", oldName, localFolder.Name));
                            throw new InteractionNeededException(string.Format("Server denied renaming of {0}", oldName), e) {
                                Title = string.Format("Server denied renaming of {0}", oldName),
                                Description = string.Format("Server denied to rename {0} to {1}, perhaps because it contains UTF-8 characters", oldName, localFolder.Name)
                            };
                        }

                        throw;
                    }
                    mappedObject.Name = remoteFolder.Name;
                    OperationsLogger.Info(string.Format("Renamed remote folder {0} with id {2} to {1}", oldName, remoteFolder.Id, remoteFolder.Name));
                } else {
                    string oldName = localFolder.Name;
                    localFolder.MoveTo(Path.Combine(localFolder.Parent.FullName, remoteFolder.Name));
                    mappedObject.Name = remoteFolder.Name;
                    OperationsLogger.Info(string.Format("Renamed local folder {0} to {1}", Path.Combine(localFolder.Parent.FullName, oldName), remoteFolder.Name));
                }

                mappedObject.LastLocalWriteTimeUtc = localFolder.LastWriteTimeUtc;
                mappedObject.LastRemoteWriteTimeUtc = (DateTime)remoteFolder.LastModificationDate;
                mappedObject.LastChangeToken = remoteFolder.ChangeToken;
                mappedObject.Ignored = remoteFolder.AreAllChildrenIgnored();
                this.Storage.SaveMappedObject(mappedObject);
            } else if (localFileSystemInfo is IFileInfo) {
                var localFile = localFileSystemInfo as IFileInfo;
                var remoteFile = remoteId as IDocument;
                var mappedObject = this.Storage.GetObjectByRemoteId(remoteFile.Id);
                if (localFile.Name.Equals(remoteFile.Name)) {
                    mappedObject.Name = localFile.Name;
                    this.Storage.SaveMappedObject(mappedObject);
                    this.changeChangeSolver.Solve(localFileSystemInfo, remoteId, localContent, remoteContent);
                } else {
                    string desc = string.Format(
                        "The local file {0} has been locally renamed from {1} to {2} and remotely to {3}. " +
                        "Fix this conflict by renaming the remote file to {2} or the local file to {3}.",
                        localFile.FullName,
                        mappedObject.Name,
                        localFile.Name,
                        remoteFile.Name);
                    OperationsLogger.Warn("Synchronization Conflict: " + desc);
                    throw new InteractionNeededException("Synchronization Conflict") {
                        Title = "Synchronization Conflict",
                        Description = desc
                    };
                }
            }
        }
    }
}