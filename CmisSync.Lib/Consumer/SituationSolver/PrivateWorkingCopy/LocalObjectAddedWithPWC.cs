//-----------------------------------------------------------------------
// <copyright file="LocalObjectAddedWithPWC.cs" company="GRAU DATA AG">
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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Cryptography;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Client.Impl;
    using DotCMIS.Data.Impl;
    using DotCMIS.Enums;
    using DotCMIS.Exceptions;

    using log4net;

    /// <summary>
    /// Local object added and the server is able to update PWC. If a folder is added => calls the given local folder added solver implementation
    /// </summary>
    public class LocalObjectAddedWithPWC : AbstractEnhancedSolverWithPWC {
        private readonly ISolver folderOrEmptyFileAddedSolver;
        private readonly ActiveActivitiesManager transmissionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Consumer.SituationSolver.PWC.LocalObjectAddedWithPWC"/> class.
        /// </summary>
        /// <param name="session">Cmis session.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="transmissionStorage">Transmission storage.</param>
        /// <param name="manager">Active activities manager.</param>
        /// <param name="localFolderOrEmptyFileAddedSolver">Local folder or empty file added solver.</param>
        public LocalObjectAddedWithPWC(
            ISession session,
            IMetaDataStorage storage,
            IFileTransmissionStorage transmissionStorage,
            ActiveActivitiesManager manager,
            ISolver localFolderOrEmptyFileAddedSolver) : base(session, storage, transmissionStorage)
        {
            if (localFolderOrEmptyFileAddedSolver == null) {
                throw new ArgumentNullException("Given solver for locally added folders is null");
            }

            if (!session.ArePrivateWorkingCopySupported()) {
                throw new ArgumentException("Given session doesn't support private working copies");
            }

            this.folderOrEmptyFileAddedSolver = localFolderOrEmptyFileAddedSolver;
            this.transmissionManager = manager;
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
            ContentChangeType localContent = ContentChangeType.NONE,
            ContentChangeType remoteContent = ContentChangeType.NONE)
        {
            if (localFileSystemInfo is IFileInfo) {
                IFileInfo localFile = localFileSystemInfo as IFileInfo;
                localFile.Refresh();
                if (!localFile.Exists) {
                    throw new FileNotFoundException(string.Format("Local file {0} has been renamed/moved/deleted", localFile.FullName));
                }

                if (localFile.Length == 0) {
                    this.folderOrEmptyFileAddedSolver.Solve(localFileSystemInfo, null, localContent, remoteContent);
                    return;
                }

                string parentId = this.Storage.GetRemoteId(localFile.Directory);
                if (parentId == null) {
                    if (this.IsParentReadOnly(localFile)) {
                        return;
                    } else {
                        throw new ArgumentException("ParentId is null => invoke crawl sync to create parent first");
                    }
                }

                IDocument remoteDocument;
                try {
                    remoteDocument = this.CreateOrLoadExistingRemoteDocument(localFile, new ObjectId(parentId));
                } catch (CmisPermissionDeniedException e) {
                    OperationsLogger.Warn(string.Format("Permission denied while trying to Create the locally added object {0} on the server ({1}).", localFile.FullName, e.Message));
                    return;
                }

                Guid uuid = this.WriteOrUseUuidIfSupported(localFile);

                FileTransmissionEvent transmissionEvent = new FileTransmissionEvent(FileTransmissionType.UPLOAD_NEW_FILE, localFile.FullName);
                this.transmissionManager.AddTransmission(transmissionEvent);

                MappedObject mapped = new MappedObject(
                    localFile.Name,
                    remoteDocument.Id,
                    MappedObjectType.File,
                    parentId,
                    remoteDocument.ChangeToken) {
                        Guid = uuid,
                        LastRemoteWriteTimeUtc = remoteDocument.LastModificationDate,
                        LastLocalWriteTimeUtc = (DateTime?)localFileSystemInfo.LastWriteTimeUtc,
                        LastChangeToken = remoteDocument.ChangeToken,
                        LastContentSize = 0,
                        ChecksumAlgorithmName = "SHA-1",
                        LastChecksum = SHA1.Create().ComputeHash(new byte[0])
                    };

                Stopwatch watch = new Stopwatch();
                OperationsLogger.Debug(string.Format("Uploading file content of {0}", localFile.FullName));
                watch.Start();
                try {
                    mapped.LastChecksum = this.UploadFileWithPWC(localFile, ref remoteDocument, transmissionEvent);
                    mapped.ChecksumAlgorithmName = "SHA-1";
                    mapped.RemoteObjectId = remoteDocument.Id;
                } catch (Exception ex) {
                    if (ex is UploadFailedException && (ex as UploadFailedException).InnerException is CmisStorageException) {
                        OperationsLogger.Warn(string.Format("Could not upload file content of {0}:", localFile.FullName), (ex as UploadFailedException).InnerException);
                        return;
                    }

                    throw;
                }
                watch.Stop();

                mapped.LastContentSize = localFile.Length;
                mapped.LastChangeToken = remoteDocument.ChangeToken;
                mapped.LastRemoteWriteTimeUtc = remoteDocument.LastModificationDate;
                mapped.LastLocalWriteTimeUtc = localFileSystemInfo.LastWriteTimeUtc;

                this.Storage.SaveMappedObject(mapped);
                OperationsLogger.Info(string.Format("Uploaded file content of {0} in [{1} msec]", localFile.FullName, watch.ElapsedMilliseconds));
            } else {
                this.folderOrEmptyFileAddedSolver.Solve(localFileSystemInfo, remoteId, localContent, remoteContent);
            }
        }

        private IDocument CreateOrLoadExistingRemoteDocument(IFileInfo localFile, IObjectId parentId) {
            IDocument result;

            IFileTransmissionObject transmissionObject = this.TransmissionStorage.GetObjectByLocalPath(localFile.FullName);
            if (transmissionObject != null) {
                try {
                    result = this.Session.GetObject(transmissionObject.RemoteObjectId) as IDocument;
                } catch (CmisObjectNotFoundException) {
                    this.TransmissionStorage.RemoveObjectByRemoteObjectId(transmissionObject.RemoteObjectId);
                    result = this.CreateCheckedOutDocument(localFile, parentId);
                }
            } else {
                result = this.CreateCheckedOutDocument(localFile, parentId);
            }

            return result;
        }

        private IDocument CreateCheckedOutDocument(IFileInfo localFile, IObjectId parentId) {
            IDocument result;
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, localFile.Name);
            properties.Add(PropertyIds.ObjectTypeId, BaseTypeId.CmisDocument.GetCmisValue());
            if (this.ServerCanModifyDateTimes) {
                properties.Add(PropertyIds.CreationDate, localFile.CreationTimeUtc);
                properties.Add(PropertyIds.LastModificationDate, localFile.LastWriteTimeUtc);
            }

            try {
                var objId = this.Session.CreateDocument(
                    properties,
                    parentId,
                    null,
                    VersioningState.CheckedOut);
                result = this.Session.GetObject(objId) as IDocument;
            } catch (CmisConstraintException e) {
                this.EnsureThatLocalFileNameContainsLegalCharacters(localFile, e);
                throw;
            }

            OperationsLogger.Info(string.Format("Created remote private working copy document {0} for {1}", result.Id, localFile.FullName));
            return result;
        }
    }
}