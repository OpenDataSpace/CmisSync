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
    using System.IO;
    using System.Security.Cryptography;

    using CmisSync.Lib.Consumer;
    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Producer.Watcher;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Client.Impl;
    using DotCMIS.Enums;

    using log4net;

    /// <summary>
    /// Solver to handle the situation of a locally added file/folderobject.
    /// </summary>
    public class LocalObjectAdded : ISolver
    {
        private static readonly ILog OperationsLogger = LogManager.GetLogger("OperationsLogger");
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LocalObjectAdded));
        private ISyncEventQueue queue;
        private ActiveActivitiesManager transmissionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Consumer.SituationSolver.LocalObjectAdded"/> class.
        /// </summary>
        /// <param name="queue">Queue to report transmission events to.</param>
        public LocalObjectAdded(ISyncEventQueue queue, ActiveActivitiesManager manager) {
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
        /// <param name="session">Cmis session instance.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="localFileSystemInfo">Local file.</param>
        /// <param name="remoteId">Remote identifier.</param>
        public void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFileSystemInfo, IObjectId remoteId)
        {
            string parentId = this.GetParentId(localFileSystemInfo, storage);
            Guid uuid = WriteUuidToExtendedAttributeIfSupported(localFileSystemInfo);

            ICmisObject addedObject = this.AddCmisObject(localFileSystemInfo, parentId, session);
            OperationsLogger.Info(string.Format("Created remote {2} {0} for {1}", addedObject.Id, localFileSystemInfo.FullName, addedObject is IFolder ? "folder" : "document"));

            if(addedObject.LastModificationDate != null) {
                try{
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
            storage.SaveMappedObject(mapped);

            var localFile = localFileSystemInfo as IFileInfo;

            if (localFile != null) {
                FileTransmissionEvent transmissionEvent = new FileTransmissionEvent(FileTransmissionType.UPLOAD_NEW_FILE, localFile.FullName);
                this.queue.AddEvent(transmissionEvent);
                this.transmissionManager.AddTransmission(transmissionEvent);
                if (localFile.Length > 0) {
                    OperationsLogger.Debug(string.Format("Uploading file content of {0}", localFile.FullName));
                    IFileUploader uploader = ContentTaskUtils.CreateUploader();
                    using (SHA1 hashAlg = new SHA1Managed())
                    using(var fileStream = localFile.Open(FileMode.Open, FileAccess.Read)) {
                        uploader.UploadFile(addedObject as IDocument, fileStream, transmissionEvent, hashAlg);
                        mapped.ChecksumAlgorithmName = "SHA1";
                        mapped.LastChecksum = hashAlg.Hash;
                    }

                    mapped.LastContentSize = localFile.Length;
                    localFileSystemInfo.LastWriteTimeUtc = addedObject.LastModificationDate != null ? (DateTime)addedObject.LastModificationDate : localFileSystemInfo.LastWriteTimeUtc;
                    mapped.LastChangeToken = addedObject.ChangeToken;
                    mapped.LastRemoteWriteTimeUtc = addedObject.LastModificationDate;
                    mapped.LastLocalWriteTimeUtc = localFileSystemInfo.LastWriteTimeUtc;

                    storage.SaveMappedObject(mapped);
                    OperationsLogger.Info(string.Format("Uploaded file content of {0}", localFile.FullName));
                }

                transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { Completed = true });
            }
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

            IMappedObject mappedParent = storage.GetObjectByLocalPath(parent);
            return mappedParent.RemoteObjectId;
        }

        private ICmisObject AddCmisObject(IFileSystemInfo localFile, string parentId, ISession session)
        {
            string name = localFile.Name;
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, name);
            if (localFile is IDirectoryInfo) {
                properties.Add(PropertyIds.ObjectTypeId, "cmis:folder");
                return session.GetObject(session.CreateFolder(properties, new ObjectId(parentId)));
            } else {
                properties.Add(PropertyIds.ObjectTypeId, "cmis:document");
                return session.GetObject(session.CreateDocument(properties, new ObjectId(parentId), null, null, null, null, null));
            }
        }
    }
}
