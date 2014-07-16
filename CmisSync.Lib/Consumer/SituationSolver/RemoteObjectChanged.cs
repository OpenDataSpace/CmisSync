//-----------------------------------------------------------------------
// <copyright file="RemoteObjectChanged.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.FileSystem;
    using CmisSync.Lib.Storage.Database;

    using DotCMIS.Client;

    using log4net;

    /// <summary>
    /// Remote object has been changed. => update the metadata locally.
    /// </summary>
    public class RemoteObjectChanged : ISolver
    {
        private static readonly ILog OperationsLogger = LogManager.GetLogger("OperationsLogger");

        private ISyncEventQueue queue;
        private IFileSystemInfoFactory fsFactory;
        private ActiveActivitiesManager transmissonManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Consumer.SituationSolver.RemoteObjectChanged"/> class.
        /// </summary>
        /// <param name="queue">Event Queue to report transmission events to.</param>
        /// <param name="fsFactory">File System Factory.</param>
        public RemoteObjectChanged(ISyncEventQueue queue, ActiveActivitiesManager transmissonManager, IFileSystemInfoFactory fsFactory = null)
        {
            if (queue == null) {
                throw new ArgumentNullException("Given queue is null");
            }

            if (transmissonManager == null) {
                throw new ArgumentNullException("Given transmission manager is null");
            }

            this.queue = queue;
            this.transmissonManager = transmissonManager;
            this.fsFactory = fsFactory ?? new FileSystemInfoFactory();
        }

        /// <summary>
        /// Solve the specified situation by using the session, storage, localFile and remoteId.
        /// If a folder is affected, simply update the local change time of the corresponding local folder.
        /// If it is a file and the changeToken is not equal to the saved, the new content is downloaded.
        /// </summary>
        /// <param name="session">Cmis session instance.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="localFile">Local file.</param>
        /// <param name="remoteId">Remote identifier.</param>
        public virtual void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFile, IObjectId remoteId)
        {
            IMappedObject obj = storage.GetObjectByRemoteId(remoteId.Id);
            if (remoteId is IFolder) {
                var remoteFolder = remoteId as IFolder;
                DateTime? lastModified = remoteFolder.LastModificationDate;
                obj.LastChangeToken = remoteFolder.ChangeToken;
                if (lastModified != null) {
                    localFile.LastWriteTimeUtc = (DateTime)lastModified;
                    obj.LastLocalWriteTimeUtc = localFile.LastWriteTimeUtc;
                }
            } else if (remoteId is IDocument) {
                byte[] lastChecksum = obj.LastChecksum;
                var remoteDocument = remoteId as IDocument;
                DateTime? lastModified = remoteDocument.LastModificationDate;
                if ((lastModified != null && lastModified != obj.LastRemoteWriteTimeUtc) || obj.LastChangeToken != remoteDocument.ChangeToken) {
                    if (obj.LastLocalWriteTimeUtc != localFile.LastWriteTimeUtc) {
                        throw new ArgumentException("The local file has been changed since last write => aborting update");
                    }

                    // Download changes
                    var file = localFile as IFileInfo;
                    var cacheFile = this.fsFactory.CreateFileInfo(file.FullName + ".sync");
                    var transmissionEvent = new FileTransmissionEvent(FileTransmissionType.DOWNLOAD_MODIFIED_FILE, localFile.FullName, cacheFile.FullName);
                    this.queue.AddEvent(transmissionEvent);
                    this.transmissonManager.AddTransmission(transmissionEvent);
                    using (SHA1 hashAlg = new SHA1Managed())
                    using (var filestream = cacheFile.Open(FileMode.Create, FileAccess.Write, FileShare.None))
                    using (IFileDownloader download = ContentTaskUtils.CreateDownloader()) {
                        try {
                            download.DownloadFile(remoteDocument, filestream, transmissionEvent, hashAlg);
                        } catch(Exception ex) {
                            transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { FailedException = ex });
                            throw;
                        }

                        obj.ChecksumAlgorithmName = "SHA1";
                        obj.LastChecksum = hashAlg.Hash;
                    }

                    var backupFile = this.fsFactory.CreateFileInfo(file.FullName + ".bak.sync");
                    string uuid = file.GetExtendedAttribute(MappedObject.ExtendedAttributeKey);
                    cacheFile.Replace(file, backupFile, true);
                    file.SetExtendedAttribute(MappedObject.ExtendedAttributeKey, uuid);
                    backupFile.SetExtendedAttribute(MappedObject.ExtendedAttributeKey, null);
                    byte[] checksumOfOldFile = null;
                    using (var oldFileStream = backupFile.Open(FileMode.Open, FileAccess.Read, FileShare.None)) {
                        checksumOfOldFile = SHA1Managed.Create().ComputeHash(oldFileStream);
                    }
                    if (!lastChecksum.SequenceEqual(checksumOfOldFile)) {
                        var conflictFile = this.fsFactory.CreateConflictFileInfo(file);
                        backupFile.MoveTo(conflictFile.FullName);
                        OperationsLogger.Info(string.Format("Updated local content of \"{0}\" with content of remote document {1} and created conflict file {2}", file.FullName, remoteId.Id, conflictFile.FullName));
                    } else {
                        backupFile.Delete();
                        OperationsLogger.Info(string.Format("Updated local content of \"{0}\" with content of remote document {1}", file.FullName, remoteId.Id));
                    }

                    obj.LastRemoteWriteTimeUtc = remoteDocument.LastModificationDate;
                    if (remoteDocument.LastModificationDate != null) {
                        localFile.LastWriteTimeUtc = (DateTime)remoteDocument.LastModificationDate;
                    }

                    obj.LastLocalWriteTimeUtc = localFile.LastWriteTimeUtc;
                    obj.LastContentSize = (long)remoteDocument.ContentStreamLength;
                    transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { Completed = true });
                }

                obj.LastChangeToken = remoteDocument.ChangeToken;
                obj.LastRemoteWriteTimeUtc = lastModified;

            }

            storage.SaveMappedObject(obj);
        }
    }
}
