//-----------------------------------------------------------------------
// <copyright file="RemoteObjectAdded.cs" company="GRAU DATA AG">
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
    using System.Security.Cryptography;

    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.FileSystem;
    using CmisSync.Lib.Storage.Database;
    
    using DotCMIS.Client;
  
    using log4net;
 
    /// <summary>
    /// Solver to handle a new object which has been found on the server
    /// </summary>
    public class RemoteObjectAdded : AbstractEnhancedSolver
    {
        private static readonly ILog OperationsLogger = LogManager.GetLogger("OperationsLogger");

        private ISyncEventQueue queue;
        private IFileSystemInfoFactory fsFactory;
        private ActiveActivitiesManager manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Consumer.SituationSolver.RemoteObjectAdded"/> class.
        /// </summary>
        /// <param name="queue">Queue to report new transmissions to.</param>
        /// <param name="fsFactory">File system factory.</param>
        public RemoteObjectAdded(
            ISession session,
            IMetaDataStorage storage,
            ISyncEventQueue queue,
            ActiveActivitiesManager transmissonManager,
            IFileSystemInfoFactory fsFactory = null) : base(session, storage) {
            if (queue == null) {
                throw new ArgumentNullException("Given queue is null");
            }

            if (transmissonManager == null) {
                throw new ArgumentNullException("Given transmission manager is null");
            }

            this.fsFactory = fsFactory ?? new FileSystemInfoFactory();
            this.queue = queue;
            this.manager = transmissonManager;
        }

        /// <summary>
        /// Adds the Object to Disk and Database
        /// </summary>
        /// <param name='localFile'>
        /// Local file.
        /// </param>
        /// <param name='remoteId'>
        /// Remote Object (already fetched).
        /// </param>
        /// <exception cref='ArgumentException'>
        /// Is thrown when remoteId is not prefetched.
        /// </exception>
        public override void Solve(
            IFileSystemInfo localFile,
            IObjectId remoteId,
            ContentChangeType localContent = ContentChangeType.NONE,
            ContentChangeType remoteContent = ContentChangeType.NONE)
        {
            if(localFile is IDirectoryInfo) {
                if(!(remoteId is IFolder)) {
                    throw new ArgumentException("remoteId has to be a prefetched Folder");
                }

                var remoteFolder = remoteId as IFolder;
                IDirectoryInfo localFolder = localFile as IDirectoryInfo;
                localFolder.Create();

                if(remoteFolder.LastModificationDate != null) {
                    localFolder.LastWriteTimeUtc = (DateTime)remoteFolder.LastModificationDate;
                }

                Guid uuid = Guid.Empty;
                if (localFolder.IsExtendedAttributeAvailable()) {
                    uuid = Guid.NewGuid();
                    localFolder.SetExtendedAttribute(MappedObject.ExtendedAttributeKey, uuid.ToString(), true);
                }

                var mappedObject = new MappedObject(remoteFolder);
                mappedObject.Guid = uuid;
                mappedObject.LastRemoteWriteTimeUtc = remoteFolder.LastModificationDate;
                mappedObject.LastLocalWriteTimeUtc = localFolder.LastWriteTimeUtc;
                this.Storage.SaveMappedObject(mappedObject);
                OperationsLogger.Info(string.Format("New local folder {0} created and mapped to remote folder {1}", localFolder.FullName, remoteId.Id));
            } else if (localFile is IFileInfo) {
                var file = localFile as IFileInfo;
                if (!(remoteId is IDocument)) {
                    throw new ArgumentException("remoteId has to be a prefetched Document");
                }

                var cacheFile = this.fsFactory.CreateFileInfo(Path.Combine(file.Directory.FullName, file.Name + ".sync"));

                IDocument remoteDoc = remoteId as IDocument;
                var transmissionEvent = new FileTransmissionEvent(FileTransmissionType.DOWNLOAD_NEW_FILE, localFile.FullName, cacheFile.FullName);
                this.queue.AddEvent(transmissionEvent);
                this.manager.AddTransmission(transmissionEvent);
                byte[] hash = null;
                using (var hashAlg = new SHA1Managed())
                using (var fileStream = cacheFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))
                using (var downloader = FileTransmission.ContentTaskUtils.CreateDownloader())
                {
                    try {
                        downloader.DownloadFile(remoteDoc, fileStream, transmissionEvent, hashAlg);
                    } catch(Exception ex) {
                        transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { FailedException = ex });
                        throw;
                    }

                    hash = hashAlg.Hash;
                }

                Guid guid = Guid.NewGuid();
                cacheFile.SetExtendedAttribute(MappedObject.ExtendedAttributeKey, guid.ToString(), false);
                try {
                    cacheFile.MoveTo(file.FullName);
                } catch (IOException) {
                    file.Refresh();
                    if (file.Exists) {
                        IFileInfo conflictFile = this.fsFactory.CreateConflictFileInfo(file);
                        IFileInfo targetFile = cacheFile.Replace(file, conflictFile, true);
                        targetFile.SetExtendedAttribute(MappedObject.ExtendedAttributeKey, guid.ToString(), true);
                        conflictFile.SetExtendedAttribute(MappedObject.ExtendedAttributeKey, null, true);
                    } else {
                        throw;
                    }
                }

                file.Refresh();
                if (remoteDoc.LastModificationDate != null) {
                    file.LastWriteTimeUtc = (DateTime)remoteDoc.LastModificationDate;
                }

                MappedObject mappedObject = new MappedObject(
                    file.Name,
                    remoteDoc.Id,
                    MappedObjectType.File,
                    remoteDoc.Parents[0].Id,
                    remoteDoc.ChangeToken,
                    remoteDoc.ContentStreamLength ?? 0)
                {
                    Guid = guid,
                    LastLocalWriteTimeUtc = file.LastWriteTimeUtc,
                    LastRemoteWriteTimeUtc = remoteDoc.LastModificationDate,
                    LastChecksum = hash,
                    ChecksumAlgorithmName = "SHA-1"
                };
                this.Storage.SaveMappedObject(mappedObject);
                OperationsLogger.Info(string.Format("New local file {0} created and mapped to remote file {1}", file.FullName, remoteId.Id));
                transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { Completed = true });
            }
        }
    }
}