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

namespace CmisSync.Lib.Sync.Solver
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;

    using CmisSync.Lib.ContentTasks;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;

    using DotCMIS.Client;

    /// <summary>
    /// A local object has been changed and should be uploaded (if necessary) to server or updated on the server.
    /// </summary>
    public class LocalObjectChanged : ISolver
    {
        private ISyncEventQueue queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Sync.Solver.LocalObjectChanged"/> class.
        /// </summary>
        /// <param name="queue">Event queue for publishing upload transmission.</param>
        public LocalObjectChanged(ISyncEventQueue queue) {
            if (queue == null) {
                throw new ArgumentNullException("Given queue is null");
            }

            this.queue = queue;
        }

        /// <summary>
        /// Solve the specified situation by using the storage, localFile and remoteId.
        /// Uploads the file content if content has been changed. Otherwise simply saves the
        /// last modification date.
        /// </summary>
        /// <param name="session">Cmis session instance.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="localFileSystemInfo">Local file system info.</param>
        /// <param name="remoteId">Remote identifier.</param>
        public virtual void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFileSystemInfo, IObjectId remoteId)
        {
            // Match local changes to remote changes and updated them remotely
            var mappedObject = storage.GetObjectByLocalPath(localFileSystemInfo);
            IFileInfo localFile = localFileSystemInfo as IFileInfo;
            if (localFile != null) {
                bool isChanged = false;
                if (localFile.Length == mappedObject.LastContentSize) {
                    using (var file = localFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read)) {
                        byte[] fileHash = SHA1Managed.Create().ComputeHash(file);
                        isChanged = !fileHash.SequenceEqual(mappedObject.LastChecksum);
                    }
                } else {
                    isChanged = true;
                }

                if (isChanged) {
                    IFileUploader uploader = ContentTasks.ContentTaskUtils.CreateUploader();
                    FileTransmissionEvent statusEvent = new FileTransmissionEvent(FileTransmissionType.UPLOAD_MODIFIED_FILE, localFile.FullName);
                    this.queue.AddEvent(statusEvent);
                    statusEvent.ReportProgress(new TransmissionProgressEventArgs { Started = true });
                    using (var hashAlg = new SHA1Managed())
                    using (var file = localFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read)) {
                        uploader.UploadFile(remoteId as IDocument, file, statusEvent, hashAlg);
                        mappedObject.LastChecksum = hashAlg.Hash;
                    }
                    statusEvent.ReportProgress(new TransmissionProgressEventArgs { Completed = true });
                }
            }

            mappedObject.LastLocalWriteTimeUtc = localFileSystemInfo.LastWriteTimeUtc;
            storage.SaveMappedObject(mappedObject);
        }
    }
}
