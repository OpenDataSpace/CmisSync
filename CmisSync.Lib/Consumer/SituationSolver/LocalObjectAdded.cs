//-----------------------------------------------------------------------
// <copyright file="LocalObjectAdded.cs" company="GRAU DATA AG">
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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Cryptography;

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Consumer;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Producer.Watcher;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Client.Impl;
    using DotCMIS.Data;
    using DotCMIS.Data.Impl;
    using DotCMIS.Enums;
    using DotCMIS.Exceptions;

    using log4net;

    /// <summary>
    /// Solver to handle the situation of a locally added file/folderobject.
    /// </summary>
    public class LocalObjectAdded : AbstractEnhancedSolver {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LocalObjectAdded));
        private TransmissionManager transmissionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Consumer.SituationSolver.LocalObjectAdded"/> class.
        /// </summary>
        /// <param name="session">Cmis session.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="transmissionStorage">File transmission storage.</param>
        /// <param name="manager">Activitiy manager for transmission propagations</param>
        public LocalObjectAdded(
            ISession session,
            IMetaDataStorage storage,
            IFileTransmissionStorage transmissionStorage,
            TransmissionManager manager) : base(session, storage, transmissionStorage)
        {
            if (manager == null) {
                throw new ArgumentNullException("Given transmission manager is null");
            }

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
            Stopwatch completewatch = new Stopwatch();
            completewatch.Start();
            Logger.Debug("Starting LocalObjectAdded");
            localFileSystemInfo.Refresh();
            if (!localFileSystemInfo.Exists) {
                throw new FileNotFoundException(string.Format("Local file/folder {0} has been renamed/moved/deleted", localFileSystemInfo.FullName));
            }

            string parentId = Storage.GetRemoteId(this.GetParent(localFileSystemInfo));
            if (parentId == null) {
                if (this.IsParentReadOnly(localFileSystemInfo)) {
                    return;
                } else {
                    throw new ArgumentException("ParentId is null => invoke crawl sync to create parent first");
                }
            }

            ICmisObject addedObject;
            try {
                addedObject = this.AddCmisObject(localFileSystemInfo, parentId, this.Session);
            } catch (CmisConstraintException e) {
                this.EnsureThatLocalFileNameContainsLegalCharacters(localFileSystemInfo, e);
                throw;
            } catch (CmisPermissionDeniedException e) {
                OperationsLogger.Warn(string.Format("Permission denied while trying to Create the locally added object {0} on the server ({1}).", localFileSystemInfo.FullName, e.Message));
                return;
            }

            Guid uuid = this.WriteOrUseUuidIfSupported(localFileSystemInfo);

            OperationsLogger.Info(string.Format("Created remote {2} {0} for {1}", addedObject.Id, localFileSystemInfo.FullName, addedObject is IFolder ? "folder" : "document"));

            MappedObject mapped = new MappedObject(
                localFileSystemInfo.Name,
                addedObject.Id,
                localFileSystemInfo is IDirectoryInfo ? MappedObjectType.Folder : MappedObjectType.File,
                parentId,
                addedObject.ChangeToken)
            {
                Guid = uuid,
                LastRemoteWriteTimeUtc = addedObject.LastModificationDate,
                LastLocalWriteTimeUtc = localFileSystemInfo is IFileInfo && (localFileSystemInfo as IFileInfo).Length > 0 ? (DateTime?)null : (DateTime?)localFileSystemInfo.LastWriteTimeUtc,
                LastChangeToken = addedObject.ChangeToken,
                LastContentSize = localFileSystemInfo is IDirectoryInfo ? -1 : 0,
                ChecksumAlgorithmName = localFileSystemInfo is IDirectoryInfo ? null : "SHA-1",
                LastChecksum = localFileSystemInfo is IDirectoryInfo ? null : SHA1.Create().ComputeHash(new byte[0])
            };
            this.Storage.SaveMappedObject(mapped);

            var localFile = localFileSystemInfo as IFileInfo;

            if (localFile != null) {
                TransmissionController transmission = new TransmissionController(TransmissionType.UPLOAD_NEW_FILE, localFile.FullName);
                this.transmissionManager.AddTransmission(transmission);
                if (localFile.Length > 0) {
                    Stopwatch watch = new Stopwatch();
                    OperationsLogger.Debug(string.Format("Uploading file content of {0}", localFile.FullName));
                    watch.Start();
                    try {
                        mapped.LastChecksum = this.UploadFile(localFile, addedObject as IDocument, transmission);
                        mapped.ChecksumAlgorithmName = "SHA-1";
                    } catch (Exception ex) {
                        if (ex is UploadFailedException && (ex as UploadFailedException).InnerException is CmisStorageException) {
                            OperationsLogger.Warn(string.Format("Could not upload file content of {0}:", localFile.FullName), (ex as UploadFailedException).InnerException);
                            return;
                        }

                        throw;
                    }

                    watch.Stop();

                    if (this.ServerCanModifyDateTimes) {
                        (addedObject as IDocument).UpdateLastWriteTimeUtc(localFile.LastWriteTimeUtc);
                    }

                    mapped.LastContentSize = localFile.Length;
                    mapped.LastChangeToken = addedObject.ChangeToken;
                    mapped.LastRemoteWriteTimeUtc = addedObject.LastModificationDate;
                    mapped.LastLocalWriteTimeUtc = localFileSystemInfo.LastWriteTimeUtc;

                    this.Storage.SaveMappedObject(mapped);
                    OperationsLogger.Info(string.Format("Uploaded file content of {0} in [{1} msec]", localFile.FullName, watch.ElapsedMilliseconds));
                } else {
                    transmission.Status = TransmissionStatus.FINISHED;
                }
            }

            completewatch.Stop();
            Logger.Debug(string.Format("Finished LocalObjectAdded after [{0} msec]", completewatch.ElapsedMilliseconds));
        }

        private ICmisObject AddCmisObject(IFileSystemInfo localFile, string parentId, ISession session) {
            string name = localFile.Name;
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, name);
            if (this.ServerCanModifyDateTimes) {
                properties.Add(PropertyIds.CreationDate, localFile.CreationTimeUtc);
                properties.Add(PropertyIds.LastModificationDate, localFile.LastWriteTimeUtc);
            }

            Stopwatch watch = new Stopwatch();
            ICmisObject result;
            if (localFile is IDirectoryInfo) {
                properties.Add(PropertyIds.ObjectTypeId, BaseTypeId.CmisFolder.GetCmisValue());
                watch.Start();
                var objId = session.CreateFolder(properties, new ObjectId(parentId));
                watch.Stop();
                Logger.Debug(string.Format("CreatedFolder in [{0} msec]", watch.ElapsedMilliseconds));
                watch.Restart();
                var operationContext = OperationContextFactory.CreateContext(session, true, false, PropertyIds.Name, PropertyIds.LastModificationDate, PropertyIds.ChangeToken);
                result = session.GetObject(objId, operationContext);
                watch.Stop();
                Logger.Debug(string.Format("GetFolder in [{0} msec]", watch.ElapsedMilliseconds));
            } else {
                bool emptyFile = (localFile as IFileInfo).Length == 0;
                properties.Add(PropertyIds.ObjectTypeId, BaseTypeId.CmisDocument.GetCmisValue());
                watch.Start();
                using (var emptyStream = new MemoryStream(new byte[0])) {
                    var objId = session.CreateDocument(
                        properties,
                        new ObjectId(parentId),
                        emptyFile ? this.CreateEmptyStream(name, emptyStream) : null,
                        null,
                        null,
                        null,
                        null);
                    watch.Stop();
                    Logger.Debug(string.Format("CreatedDocument in [{0} msec]", watch.ElapsedMilliseconds));
                    watch.Restart();
                    var operationContext = OperationContextFactory.CreateContext(session, true, false, PropertyIds.Name, PropertyIds.LastModificationDate, PropertyIds.ChangeToken, PropertyIds.ContentStreamLength);
                    result = session.GetObject(objId, operationContext);
                    watch.Stop();
                    Logger.Debug(string.Format("GetDocument in [{0} msec]", watch.ElapsedMilliseconds));
                }
            }

            return result;
        }

        private IContentStream CreateEmptyStream(string filename, Stream emptyStream) {
            ContentStream contentStream = new ContentStream();
            contentStream.FileName = filename;
            contentStream.MimeType = Cmis.MimeType.GetMIMEType(filename);
            contentStream.Length = 0;
            contentStream.Stream = emptyStream;
            return contentStream;
        }
    }
}