//-----------------------------------------------------------------------
// <copyright file="LocalObjectChangedWithPWC.cs" company="GRAU DATA AG">
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
    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Exceptions;
    using DotCMIS.Client;

    using log4net;

    /// <summary>
    /// Local object changed and file content should be uploaded via PWC. Otherwise the fallback solver is called.
    /// </summary>
    public class LocalObjectChangedWithPWC : AbstractEnhancedSolverWithPWC {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LocalObjectChangedWithPWC));
        private readonly ISolver folderOrFileContentUnchangedSolver;
        private ITransmissionFactory transmissionManager;

        public LocalObjectChangedWithPWC(
            ISession session,
            IMetaDataStorage storage,
            IFileTransmissionStorage transmissionStorage,
            ITransmissionFactory manager,
            ISolver folderOrFileContentUnchangedSolver) : base(session, storage, transmissionStorage) {
            if (folderOrFileContentUnchangedSolver == null) {
                throw new ArgumentNullException("folderOrFileContentUnchangedSolver", "Given solver for folder or unchanged file content situations is null");
            }

            this.folderOrFileContentUnchangedSolver = folderOrFileContentUnchangedSolver;
            this.transmissionManager = manager;
        }

        public override void Solve(
            IFileSystemInfo localFileSystemInfo,
            IObjectId remoteId,
            ContentChangeType localContent = ContentChangeType.NONE,
            ContentChangeType remoteContent = ContentChangeType.NONE)
        {
            if (localFileSystemInfo is IFileInfo && remoteId is IDocument) {
                var localFile = localFileSystemInfo as IFileInfo;
                var remoteDocument = remoteId as IDocument;

                var mappedObject = this.Storage.GetObject(localFile);
                if (mappedObject == null) {
                    throw new ArgumentException(string.Format("Could not find db entry for {0} => invoke crawl sync", localFileSystemInfo.FullName));
                }

                if (mappedObject.LastChangeToken != (remoteId as ICmisObjectProperties).ChangeToken) {
                    throw new ArgumentException(string.Format("remote {1} {0} has also been changed since last sync => invoke crawl sync", remoteId.Id, remoteId is IDocument ? "document" : "folder"));
                }

                if (localFile != null && localFile.IsContentChangedTo(mappedObject, scanOnlyIfModificationDateDiffers: true)) {
                    Logger.Debug(string.Format("\"{0}\" is different from {1}", localFile.FullName, mappedObject.ToString()));
                    OperationsLogger.Debug(string.Format("Local file \"{0}\" has been changed", localFile.FullName));
                    try {
                        var transmission = this.transmissionManager.CreateTransmission(TransmissionType.UploadModifiedFile, localFile.FullName);
                        mappedObject.LastChecksum = this.UploadFileWithPWC(localFile, ref remoteDocument, transmission);
                        mappedObject.ChecksumAlgorithmName = "SHA-1";
                        if (remoteDocument.Id != mappedObject.RemoteObjectId) {
                            this.TransmissionStorage.RemoveObjectByRemoteObjectId(mappedObject.RemoteObjectId);
                            mappedObject.RemoteObjectId = remoteDocument.Id;
                        }
                    } catch (Exception ex) {
                        if (ex.InnerException is CmisPermissionDeniedException) {
                            OperationsLogger.Warn(string.Format("Local changed file \"{0}\" has not been uploaded: PermissionDenied", localFile.FullName));
                            return;
                        } else if (ex.InnerException is CmisStorageException) {
                            OperationsLogger.Warn(string.Format("Local changed file \"{0}\" has not been uploaded: StorageException", localFile.FullName), ex);
                            return;
                        }

                        throw;
                    }

                    mappedObject.LastRemoteWriteTimeUtc = remoteDocument.LastModificationDate;
                    mappedObject.LastLocalWriteTimeUtc = localFile.LastWriteTimeUtc;
                    mappedObject.LastContentSize = localFile.Length;

                    OperationsLogger.Info(string.Format("Local changed file \"{0}\" has been uploaded", localFile.FullName));
                }

                mappedObject.LastChangeToken = remoteDocument.ChangeToken;
                mappedObject.LastLocalWriteTimeUtc = localFile.LastWriteTimeUtc;
                this.Storage.SaveMappedObject(mappedObject);
            } else {
                this.folderOrFileContentUnchangedSolver.Solve(localFileSystemInfo, remoteId, localContent, remoteContent);
            }

            return;
        }
    }
}