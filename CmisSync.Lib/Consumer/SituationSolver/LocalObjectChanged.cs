//-----------------------------------------------------------------------
// <copyright file="LocalObjectChanged.cs" company="GRAU DATA AG">
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
    using System.Linq;
    using System.Security.Cryptography;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    using log4net;

    /// <summary>
    /// A local object has been changed and should be uploaded (if necessary) to server or updated on the server.
    /// </summary>
    public class LocalObjectChanged : AbstractEnhancedSolver
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LocalObjectChanged));
        private static readonly ILog OperationsLogger = LogManager.GetLogger("OperationsLogger");

        private ISyncEventQueue queue;
        private ActiveActivitiesManager transmissionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Consumer.SituationSolver.LocalObjectChanged"/> class.
        /// </summary>
        /// <param name="session">Cmis session.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="queue">Event queue for publishing upload transmission.</param>
        /// <param name="transmissionManager">Transmission manager.</param>
        /// <param name="serverCanModifyCreationAndModificationDate">If set to <c>true</c> server can modify creation and modification date.</param>
        public LocalObjectChanged(
            ISession session,
            IMetaDataStorage storage,
            ISyncEventQueue queue,
            ActiveActivitiesManager transmissionManager,
            bool serverCanModifyCreationAndModificationDate = false) : base(session, storage, serverCanModifyCreationAndModificationDate) {
            if (queue == null) {
                throw new ArgumentNullException("Given queue is null");
            }

            if(transmissionManager == null) {
                throw new ArgumentNullException("Given transmission manager is null");
            }

            this.queue = queue;
            this.transmissionManager = transmissionManager;
        }

        /// <summary>
        /// Solve the specified situation by using the storage, localFile and remoteId.
        /// Uploads the file content if content has been changed. Otherwise simply saves the
        /// last modification date.
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
            if (!localFileSystemInfo.Exists) {
                throw new ArgumentException("Given local path does not exists: " + localFileSystemInfo.FullName);
            }

            // Match local changes to remote changes and updated them remotely
            IMappedObject mappedObject = null;
            try {
                string ea = localFileSystemInfo.GetExtendedAttribute(MappedObject.ExtendedAttributeKey);
                Guid guid;
                if (Guid.TryParse(ea, out guid)) {
                    mappedObject = this.Storage.GetObjectByGuid(guid);
                }
            } catch(Exception) {
            }

            if (mappedObject == null) {
                mappedObject = this.Storage.GetObjectByLocalPath(localFileSystemInfo);
            }

            IFileInfo localFile = localFileSystemInfo as IFileInfo;
            if (localFile != null && localFile.IsContentChangedTo(mappedObject, scanOnlyIfModificationDateDiffers: true)) {
                Logger.Debug(string.Format("\"{0}\" is different from {1}", localFile.FullName, mappedObject.ToString()));
                OperationsLogger.Debug(string.Format("Local file \"{0}\" has been changed", localFile.FullName));
                IFileUploader uploader = FileTransmission.ContentTaskUtils.CreateUploader();
                var doc = remoteId as IDocument;
                FileTransmissionEvent transmissionEvent = new FileTransmissionEvent(FileTransmissionType.UPLOAD_MODIFIED_FILE, localFile.FullName);
                this.queue.AddEvent(transmissionEvent);
                this.transmissionManager.AddTransmission(transmissionEvent);
                transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { Started = true });
                using (var hashAlg = new SHA1Managed())
                using (var file = localFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)) {
                    try {
                        uploader.UploadFile(doc, file, transmissionEvent, hashAlg);
                    } catch(Exception ex) {
                        transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { FailedException = ex });
                        if (ex.InnerException is CmisPermissionDeniedException) {
                            OperationsLogger.Warn(string.Format("Local changed file \"{0}\" has been not been uploaded: PermissionDenied", localFile.FullName));
                            return;
                        }

                        throw;
                    }

                    mappedObject.LastChecksum = hashAlg.Hash;
                }

                mappedObject.LastChangeToken = doc.ChangeToken;
                mappedObject.LastRemoteWriteTimeUtc = doc.LastModificationDate;
                mappedObject.LastLocalWriteTimeUtc = localFile.LastWriteTimeUtc;
                mappedObject.LastContentSize = localFile.Length;

                OperationsLogger.Info(string.Format("Local changed file \"{0}\" has been uploaded", localFile.FullName));

                transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { Completed = true });
            }

            if (this.ServerCanModifyDateTimes) {
                try {
                    if (remoteId is IDocument) {
                        (remoteId as IDocument).UpdateLastWriteTimeUtc(localFileSystemInfo.LastWriteTimeUtc);
                        mappedObject.LastRemoteWriteTimeUtc = localFileSystemInfo.LastWriteTimeUtc;
                    } else if (remoteId is IFolder) {
                        (remoteId as IFolder).UpdateLastWriteTimeUtc(localFileSystemInfo.LastWriteTimeUtc);
                        mappedObject.LastRemoteWriteTimeUtc = localFileSystemInfo.LastWriteTimeUtc;
                    }
                } catch(CmisPermissionDeniedException) {
                    Logger.Debug(string.Format("Locally changed modification date \"{0}\"is not uploaded to the server: PermissionDenied", localFileSystemInfo.LastWriteTimeUtc));
                }
            }

            mappedObject.LastLocalWriteTimeUtc = localFileSystemInfo.LastWriteTimeUtc;
            this.Storage.SaveMappedObject(mappedObject);
        }
    }
}