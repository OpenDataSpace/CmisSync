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
    using DotCMIS.Enums;

    using log4net;

    /// <summary>
    /// Solver to handle the situation of a locally added file/folderobject.
    /// </summary>
    public class LocalObjectAdded : AbstractEnhancedSolver
    {
        private static readonly ILog OperationsLogger = LogManager.GetLogger("OperationsLogger");
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LocalObjectAdded));
        private ISyncEventQueue queue;
        private ActiveActivitiesManager transmissionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Consumer.SituationSolver.LocalObjectAdded"/> class.
        /// </summary>
        /// <param name="queue">Queue to report transmission events to.</param>
        /// <param name="manager">Activitiy manager for transmission propagations</param>
        public LocalObjectAdded(ISession session, IMetaDataStorage storage, ISyncEventQueue queue, ActiveActivitiesManager manager) : base(session, storage) {
            if (queue == null) {
                throw new ArgumentNullException("Given queue is null");
            }

            if (manager == null) {
                throw new ArgumentNullException("Given transmission manager is null");
            }

            this.queue = queue;
            this.transmissionManager = manager;
        }

        /// <summary>
        /// Solve the situation of a local object added and should be uploaded by using the session, storage, localFile and remoteId.
        /// </summary>
        /// <param name="localFileSystemInfo">Local file.</param>
        /// <param name="remoteId">Remote identifier.</param>
        public override void Solve(IFileSystemInfo localFileSystemInfo, IObjectId remoteId)
        {
            Stopwatch completewatch = new Stopwatch();
            completewatch.Start();
            Logger.Debug("Starting LocalObjectAdded");
            string parentId = this.GetParentId(localFileSystemInfo, this.Storage);
            Guid uuid = WriteUuidToExtendedAttributeIfSupported(localFileSystemInfo);

            ICmisObject addedObject = this.AddCmisObject(localFileSystemInfo, parentId, this.Session);
            OperationsLogger.Info(string.Format("Created remote {2} {0} for {1}", addedObject.Id, localFileSystemInfo.FullName, addedObject is IFolder ? "folder" : "document"));

            if(addedObject.LastModificationDate != null) {
                try {
                    localFileSystemInfo.LastWriteTimeUtc = (DateTime)addedObject.LastModificationDate;
                } catch (IOException e) {
                    Logger.Info("Could not write LastWriteTimeUtc due to: " + e.Message);
                }
            }

            MappedObject mapped = new MappedObject(
                localFileSystemInfo.Name,
                addedObject.Id,
                localFileSystemInfo is IDirectoryInfo ? MappedObjectType.Folder : MappedObjectType.File,
                parentId,
                addedObject.ChangeToken)
            {
                Guid = uuid,
                LastRemoteWriteTimeUtc = addedObject.LastModificationDate,
                /* TODO DIRTY DIRTY DIRTY HACK! THIS MUST BE REFACTORED
                 * The LastLocalWriteTime is not set to a value to ensure that a not completely
                 * uploaded file is recognized as changed on the next crawl sync
                 */
                LastLocalWriteTimeUtc = localFileSystemInfo is IFileInfo && (localFileSystemInfo as IFileInfo).Length > 0 ? (DateTime?)null : (DateTime?)localFileSystemInfo.LastWriteTimeUtc,
                LastChangeToken = addedObject.ChangeToken,
                LastContentSize = localFileSystemInfo is IDirectoryInfo ? -1 : 0
            };
            this.Storage.SaveMappedObject(mapped);

            var localFile = localFileSystemInfo as IFileInfo;

            if (localFile != null) {
                FileTransmissionEvent transmissionEvent = new FileTransmissionEvent(FileTransmissionType.UPLOAD_NEW_FILE, localFile.FullName);
                this.queue.AddEvent(transmissionEvent);
                this.transmissionManager.AddTransmission(transmissionEvent);
                if (localFile.Length > 0) {
                    Stopwatch watch = new Stopwatch();
                    OperationsLogger.Debug(string.Format("Uploading file content of {0}", localFile.FullName));
                    watch.Start();
                    IFileUploader uploader = ContentTaskUtils.CreateUploader();
                    using (SHA1 hashAlg = new SHA1Managed())
                    using(var fileStream = localFile.Open(FileMode.Open, FileAccess.Read)) {
                        uploader.UploadFile(addedObject as IDocument, fileStream, transmissionEvent, hashAlg);
                        mapped.ChecksumAlgorithmName = "SHA1";
                        mapped.LastChecksum = hashAlg.Hash;
                    }

                    watch.Stop();
                    mapped.LastContentSize = localFile.Length;
                    localFileSystemInfo.LastWriteTimeUtc = addedObject.LastModificationDate != null ? (DateTime)addedObject.LastModificationDate : localFileSystemInfo.LastWriteTimeUtc;
                    mapped.LastChangeToken = addedObject.ChangeToken;
                    mapped.LastRemoteWriteTimeUtc = addedObject.LastModificationDate;
                    mapped.LastLocalWriteTimeUtc = localFileSystemInfo.LastWriteTimeUtc;

                    this.Storage.SaveMappedObject(mapped);
                    OperationsLogger.Info(string.Format("Uploaded file content of {0} in [{1} msec]", localFile.FullName, watch.ElapsedMilliseconds));
                }

                transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { Completed = true });
            }

            completewatch.Stop();
            Logger.Debug(string.Format("Finished LocalObjectAdded after [{0} msec]", completewatch.ElapsedMilliseconds));
        }

        private static Guid WriteUuidToExtendedAttributeIfSupported(IFileSystemInfo localFile)
        {
            Guid uuid = Guid.Empty;
            if (localFile.IsExtendedAttributeAvailable())
            {
                uuid = Guid.NewGuid();
                try {
                    localFile.SetExtendedAttribute(MappedObject.ExtendedAttributeKey, uuid.ToString());
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
                if (Guid.TryParse(parent.GetExtendedAttribute(MappedObject.ExtendedAttributeKey), out uuid)){
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
                properties.Add(PropertyIds.ObjectTypeId, "cmis:document");
                watch.Start();
                var objId = session.CreateDocument(properties, new ObjectId(parentId), null, null, null, null, null);
                watch.Stop();
                Logger.Debug(string.Format("CreatedDocument in [{0} msec]", watch.ElapsedMilliseconds));
                watch.Restart();
                var operationContext = OperationContextFactory.CreateContext(session, true, false, "cmis:name", "cmis:lastModificationDate", "cmis:changeToken", "cmis:contentStreamLength");
                result = session.GetObject(objId, operationContext);
                watch.Stop();
                Logger.Debug(string.Format("GetDocument in [{0} msec]", watch.ElapsedMilliseconds));
            }

            watch.Stop();
            return result;
        }
    }
}
