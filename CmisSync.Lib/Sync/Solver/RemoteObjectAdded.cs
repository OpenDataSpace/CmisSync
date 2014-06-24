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

namespace CmisSync.Lib.Sync.Solver
{
    using System;
    using System.IO;
    using System.Security.Cryptography;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    
    using DotCMIS.Client;
  
    using log4net;
 
    /// <summary>
    /// Solver to handle a new object which has been found on the server
    /// </summary>
    public class RemoteObjectAdded : ISolver
    {
        private ISyncEventQueue queue;
        private IFileSystemInfoFactory fsFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Sync.Solver.RemoteObjectAdded"/> class.
        /// </summary>
        /// <param name="queue">Queue to report new transmissions to.</param>
        /// <param name="fsFactory">File system factory.</param>
        public RemoteObjectAdded(ISyncEventQueue queue, IFileSystemInfoFactory fsFactory = null) {
            if (queue == null) {
                throw new ArgumentNullException("Given queue is null");
            }

            this.fsFactory = fsFactory ?? new FileSystemInfoFactory();
            this.queue = queue;
        }

        /// <summary>
        /// Adds the Object to Disk and Database
        /// </summary>
        /// <param name='session'>
        /// Cmis Session.
        /// </param>
        /// <param name='storage'>
        /// Metadata Storage.
        /// </param>
        /// <param name='localFile'>
        /// Local file.
        /// </param>
        /// <param name='remoteId'>
        /// Remote Object (already fetched).
        /// </param>
        /// <exception cref='ArgumentException'>
        /// Is thrown when remoteId is not prefetched.
        /// </exception>
        public virtual void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFile, IObjectId remoteId) {
            if(localFile is IDirectoryInfo) {
                if(!(remoteId is IFolder)) {
                    throw new ArgumentException("remoteId has to be a prefetched Folder");
                }

                var remoteFolder = remoteId as IFolder;
                IDirectoryInfo localFolder = localFile as IDirectoryInfo;
                localFolder.Create();

                if(remoteFolder.LastModificationDate != null)
                {
                    localFolder.LastWriteTimeUtc = (DateTime)remoteFolder.LastModificationDate;
                }

                Guid uuid = Guid.Empty;
                if (localFolder.IsExtendedAttributeAvailable())
                {
                    uuid = Guid.NewGuid();
                    localFolder.SetExtendedAttribute(MappedObject.ExtendedAttributeKey, uuid.ToString());
                }

                var mappedObject = new MappedObject(remoteFolder);
                mappedObject.Guid = uuid;
                mappedObject.LastRemoteWriteTimeUtc = remoteFolder.LastModificationDate;
                mappedObject.LastLocalWriteTimeUtc = localFolder.LastWriteTimeUtc;
                storage.SaveMappedObject(mappedObject);
            } else if (localFile is IFileInfo) {
                var file = localFile as IFileInfo;
                if (!(remoteId is IDocument)) {
                    throw new ArgumentException("remoteId has to be a prefetched Document");
                }

                var cacheFile = this.fsFactory.CreateFileInfo(Path.Combine(file.Directory.FullName, file.Name + ".sync"));

                IDocument remoteDoc = remoteId as IDocument;
                var transmissionEvent = new FileTransmissionEvent(FileTransmissionType.DOWNLOAD_NEW_FILE, localFile.FullName, cacheFile.FullName);
                this.queue.AddEvent(transmissionEvent);
                byte[] hash = null;
                using (var hashAlg = new SHA1Managed())
                using (var fileStream = cacheFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))
                using (var downloader = ContentTasks.ContentTaskUtils.CreateDownloader())
                {
                    downloader.DownloadFile(remoteDoc, fileStream, transmissionEvent, hashAlg);
                    hash = hashAlg.Hash;
                }

                Guid guid = Guid.NewGuid();
                cacheFile.SetExtendedAttribute(MappedObject.ExtendedAttributeKey, guid.ToString());
                cacheFile.MoveTo(file.FullName);
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
                    ChecksumAlgorithmName = "SHA1"
                };
                storage.SaveMappedObject(mappedObject);
            }
        }
    }
}