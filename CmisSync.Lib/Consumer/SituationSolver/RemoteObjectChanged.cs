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

namespace CmisSync.Lib.Consumer.SituationSolver {
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DataSpace.Common.Transmissions;

    using DotCMIS.Client;

    using log4net;

    /// <summary>
    /// Remote object has been changed. => update the metadata locally.
    /// </summary>
    public class RemoteObjectChanged : AbstractEnhancedSolver {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RemoteObjectChanged));

        private IFileSystemInfoFactory fsFactory;
        private ITransmissionFactory transmissionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Consumer.SituationSolver.RemoteObjectChanged"/> class.
        /// </summary>
        /// <param name="session">Cmis session.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="transmissionStorage">Transmission storage.</param>
        /// <param name="transmissionFactory">Transmisson factory.</param>
        /// <param name="fsFactory">File System Factory.</param>
        public RemoteObjectChanged(
            ISession session,
            IMetaDataStorage storage,
            IFileTransmissionStorage transmissionStorage,
            ITransmissionFactory transmissionFactory,
            IFileSystemInfoFactory fsFactory = null) : base(session, storage, transmissionStorage)
        {
            if (transmissionFactory == null) {
                throw new ArgumentNullException("transmissionFactory");
            }

            this.transmissionFactory = transmissionFactory;
            this.fsFactory = fsFactory ?? new FileSystemInfoFactory();
        }

        /// <summary>
        /// Solve the specified situation by using the session, storage, localFile and remoteId.
        /// If a folder is affected, simply update the local change time of the corresponding local folder.
        /// If it is a file and the changeToken is not equal to the saved, the new content is downloaded.
        /// </summary>
        /// <param name="localFileSystemInfo">Local file.</param>
        /// <param name="remoteId">Remote identifier.</param>
        /// <param name="localContent">Hint if the local content has been changed.</param>
        /// <param name="remoteContent">Information if the remote content has been changed.</param>
        public override void Solve(
            IFileSystemInfo localFileSystemInfo,
            IObjectId remoteId,
            ContentChangeType localContent = ContentChangeType.NONE,
            ContentChangeType remoteContent = ContentChangeType.NONE)
        {
            if (remoteId == null) {
                throw new ArgumentNullException("remoteId");
            }

            var storedObject = this.Storage.GetObjectByRemoteId(remoteId.Id);
            if (remoteId is IFolder) {
                var remoteFolder = remoteId as IFolder;
                DateTime? lastModified = remoteFolder.LastModificationDate;
                storedObject.LastChangeToken = remoteFolder.ChangeToken;
                storedObject.Ignored = remoteFolder.AreAllChildrenIgnored();
                localFileSystemInfo.TryToSetReadOnlyStateIfDiffers(from: remoteFolder, andLogErrorsTo: Logger);
                storedObject.IsReadOnly = localFileSystemInfo.ReadOnly;

                localFileSystemInfo.TryToSetLastWriteTimeUtcIfAvailable(from: remoteFolder, andLogErrorsTo: Logger);
                storedObject.LastLocalWriteTimeUtc = localFileSystemInfo.LastWriteTimeUtc;
                storedObject.LastRemoteWriteTimeUtc = remoteFolder.LastModificationDate;
            } else if (remoteId is IDocument) {
                var remoteDocument = remoteId as IDocument;
                DateTime? lastModified = remoteDocument.LastModificationDate;
                if ((lastModified != null && lastModified != storedObject.LastRemoteWriteTimeUtc) || storedObject.LastChangeToken != remoteDocument.ChangeToken) {
                    if (remoteContent != ContentChangeType.NONE) {
                        if (storedObject.LastLocalWriteTimeUtc != localFileSystemInfo.LastWriteTimeUtc) {
                            throw new ArgumentException("The local file has been changed since last write => aborting update");
                        }

                        storedObject.LastChecksum = this.DownloadChanges(localFileSystemInfo as IFileInfo, remoteDocument, storedObject, this.fsFactory, this.transmissionFactory, Logger);
                    }

                    localFileSystemInfo.TryToSetReadOnlyStateIfDiffers(from: remoteDocument, andLogErrorsTo: Logger);
                    storedObject.LastRemoteWriteTimeUtc = remoteDocument.LastModificationDate;
                    localFileSystemInfo.TryToSetLastWriteTimeUtcIfAvailable(from: remoteDocument, andLogErrorsTo: Logger);
                    storedObject.LastLocalWriteTimeUtc = localFileSystemInfo.LastWriteTimeUtc;
                    storedObject.LastContentSize = remoteDocument.ContentStreamLength ?? 0;
                }

                storedObject.LastChangeToken = remoteDocument.ChangeToken;
                storedObject.LastRemoteWriteTimeUtc = lastModified;
                storedObject.IsReadOnly = localFileSystemInfo.ReadOnly;
            }

            this.Storage.SaveMappedObject(storedObject);
        }
    }
}