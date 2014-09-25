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

namespace CmisSync.Lib.Consumer.SituationSolver
{
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
    public class LocalObjectAdded : AbstractEnhancedSolver
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LocalObjectAdded));
        private ActiveActivitiesManager transmissionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Consumer.SituationSolver.LocalObjectAdded"/> class.
        /// </summary>
        /// <param name="session">Cmis session.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="manager">Activitiy manager for transmission propagations</param>
        /// <param name="serverCanModifyCreationAndModificationDate">If set to <c>true</c> server can modify creation and modification date.</param>
        public LocalObjectAdded(
            ISession session,
            IMetaDataStorage storage,
            ActiveActivitiesManager manager) : base(session, storage) {
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
            string parentId = this.GetParentId(localFileSystemInfo, this.Storage);
            Guid uuid = this.WriteOrUseUuidIfSupported(localFileSystemInfo);

            ICmisObject addedObject;
            try {
                addedObject = this.AddCmisObject(localFileSystemInfo, parentId, this.Session);
            } catch (CmisPermissionDeniedException e) {
                OperationsLogger.Warn(string.Format("Permission denied while trying to Create the locally added object {0} on the server ({1}).", localFileSystemInfo.FullName, e.Message));
                return;
            }

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
                FileTransmissionEvent transmissionEvent = new FileTransmissionEvent(FileTransmissionType.UPLOAD_NEW_FILE, localFile.FullName);
                this.transmissionManager.AddTransmission(transmissionEvent);
                if (localFile.Length > 0) {
                    Stopwatch watch = new Stopwatch();
                    OperationsLogger.Debug(string.Format("Uploading file content of {0}", localFile.FullName));
                    watch.Start();
                    IFileUploader uploader = ContentTaskUtils.CreateUploader();
                    using (SHA1 hashAlg = new SHA1Managed())
                    using(var fileStream = localFile.Open(FileMode.Open, FileAccess.Read)) {
                        try {
                            uploader.UploadFile(addedObject as IDocument, fileStream, transmissionEvent, hashAlg);
                        } catch (Exception ex) {
                            transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { FailedException = ex });
                            throw;
                        }

                        mapped.ChecksumAlgorithmName = "SHA-1";
                        mapped.LastChecksum = hashAlg.Hash;
                    }

                    watch.Stop();

                    if (this.ServerCanModifyDateTimes) {
                        (addedObject as IDocument).UpdateLastWriteTimeUtc(localFile.LastWriteTimeUtc);
                    }

                    mapped.LastContentSize = localFile.Length;
                    mapped.LastChangeToken = addedObject.ChangeToken;
                    mapped.LastRemoteWriteTimeUtc = addedObject.LastModificationDate;
                    mapped.LastLocalWriteTimeUtc = localFileSystemInfo.LastWriteTimeUtc;
                    if (mapped.RemoteObjectId != addedObject.Id) {
                        this.Storage.RemoveObject(mapped);
                        mapped.RemoteObjectId = addedObject.Id;
                    }

                    this.Storage.SaveMappedObject(mapped);
                    OperationsLogger.Info(string.Format("Uploaded file content of {0} in [{1} msec]", localFile.FullName, watch.ElapsedMilliseconds));
                }

                transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { Completed = true });
            }

            completewatch.Stop();
            Logger.Debug(string.Format("Finished LocalObjectAdded after [{0} msec]", completewatch.ElapsedMilliseconds));
        }

        private Guid WriteOrUseUuidIfSupported(IFileSystemInfo localFile)
        {
            Guid uuid = Guid.Empty;
            if (localFile.IsExtendedAttributeAvailable())
            {
                try {
                    Guid? localUuid = localFile.Uuid;
                    if (localUuid == null || this.Storage.GetObjectByGuid((Guid)localUuid) != null) {
                        uuid = Guid.NewGuid();
                        try {
                            localFile.Uuid = uuid;
                        } catch (RestoreModificationDateException restoreException) {
                            Logger.Debug("Could not retore the last modification date of " + localFile.FullName, restoreException);
                        }
                    } else {
                        uuid = (Guid)localUuid;
                    }
                } catch (ExtendedAttributeException ex) {
                    throw new RetryException(ex.Message, ex);
                }
            }

            return uuid;
        }

        private string GetParentId(IFileSystemInfo fileInfo, IMetaDataStorage storage)
        {
            IDirectoryInfo parent = null;
            if (fileInfo is IDirectoryInfo) {
                IDirectoryInfo localDirInfo = fileInfo as IDirectoryInfo;
                parent = localDirInfo.Parent;
            } else {
                IFileInfo localFileInfo = fileInfo as IFileInfo;
                parent = localFileInfo.Directory;
            }

            try {
                Guid uuid;
                if (Guid.TryParse(parent.GetExtendedAttribute(MappedObject.ExtendedAttributeKey), out uuid)) {
                    return storage.GetObjectByGuid(uuid).RemoteObjectId;
                }
            } catch (IOException) {
            }

            IMappedObject mappedParent = storage.GetObjectByLocalPath(parent);
            return mappedParent.RemoteObjectId;
        }

        private ICmisObject AddCmisObject(IFileSystemInfo localFile, string parentId, ISession session)
        {
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
                properties.Add(PropertyIds.ObjectTypeId, "cmis:folder");
                watch.Start();
                var objId = session.CreateFolder(properties, new ObjectId(parentId));
                watch.Stop();
                Logger.Debug(string.Format("CreatedFolder in [{0} msec]", watch.ElapsedMilliseconds));
                watch.Restart();
                var operationContext = OperationContextFactory.CreateContext(session, true, false, "cmis:name", "cmis:lastModificationDate", "cmis:changeToken");
                result = session.GetObject(objId, operationContext);
                watch.Stop();
                Logger.Debug(string.Format("GetFolder in [{0} msec]", watch.ElapsedMilliseconds));
            } else {
                bool emptyFile = (localFile as IFileInfo).Length == 0;
                properties.Add(PropertyIds.ObjectTypeId, "cmis:document");
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
                    var operationContext = OperationContextFactory.CreateContext(session, true, false, "cmis:name", "cmis:lastModificationDate", "cmis:changeToken", "cmis:contentStreamLength");
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