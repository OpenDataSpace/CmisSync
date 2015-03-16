//-----------------------------------------------------------------------
// <copyright file="AbstractEnhancedSolverWithPWC.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Consumer.SituationSolver.PWC {
    using System;
    using System.Linq;
    using System.IO;
    using System.Security.Cryptography;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Streams;
    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.HashAlgorithm;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using log4net;

    public abstract class AbstractEnhancedSolverWithPWC : AbstractEnhancedSolver {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AbstractEnhancedSolverWithPWC));

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Consumer.SituationSolver.AbstractEnhancedSolver"/> class.
        /// </summary>
        /// <param name="session">Cmis Session.</param>
        /// <param name="storage">Meta Data Storage.</param>
        /// <param name="transmissionStorage">File Transmission Storage.</param>
        public AbstractEnhancedSolverWithPWC(
            ISession session,
            IMetaDataStorage storage,
            IFileTransmissionStorage transmissionStorage = null) : base(session, storage, transmissionStorage) {
        }

        private IDocument CreateRemotePWCDocument(IDocument remoteDocument) {
            try {
                if (this.TransmissionStorage != null) {
                    this.TransmissionStorage.RemoveObjectByRemoteObjectId(remoteDocument.Id);
                }

                if (!string.IsNullOrEmpty(remoteDocument.VersionSeriesCheckedOutId)) {
                    remoteDocument.CancelCheckOut();
                    remoteDocument.Refresh();
                }

                remoteDocument.CheckOut();
                remoteDocument.Refresh();
                IDocument remotePWCDocument = this.Session.GetObject(remoteDocument.VersionSeriesCheckedOutId) as IDocument;
                remotePWCDocument.DeleteContentStream();
                return remotePWCDocument;
            } catch (Exception ex) {
                return null;
            }
        }

        private IDocument LoadRemotePWCDocument(IDocument remoteDocument, ref byte[] checksum) {
            if (this.TransmissionStorage == null) {
                return this.CreateRemotePWCDocument(remoteDocument);
            }

            IFileTransmissionObject obj = this.TransmissionStorage.GetObjectByRemoteObjectId(remoteDocument.Id);
            if (obj == null) {
                return this.CreateRemotePWCDocument(remoteDocument);
            }

            if (obj.RemoteObjectPWCId != remoteDocument.VersionSeriesCheckedOutId) {
                return this.CreateRemotePWCDocument(remoteDocument);
            }

            IDocument remotePWCDocument = this.Session.GetObject(remoteDocument.VersionSeriesCheckedOutId) as IDocument;
            if (remotePWCDocument == null) {
                return this.CreateRemotePWCDocument(remoteDocument);
            }

            if (remotePWCDocument.ChangeToken != obj.LastChangeTokenPWC) {
                return this.CreateRemotePWCDocument(remoteDocument);
            }

            this.TransmissionStorage.RemoveObjectByRemoteObjectId(remoteDocument.Id);

            checksum = obj.LastChecksumPWC;
            return remotePWCDocument;
        }

        private void SaveRemotePWCDocument(IFileInfo localFile, IDocument remoteDocument, IDocument remotePWCDocument, byte[] checksum, FileTransmissionEvent transmissionEvent) {
            if (this.TransmissionStorage == null) {
                return;
            }

            if (remotePWCDocument == null) {
                return;
            }

            FileTransmissionObject obj = new FileTransmissionObject(transmissionEvent.Type, localFile, remoteDocument);
            obj.ChecksumAlgorithmName = "SHA-1";
            obj.RemoteObjectPWCId = remotePWCDocument.Id;
            remotePWCDocument.Refresh();
            obj.LastChangeTokenPWC = remotePWCDocument.ChangeToken;
            obj.LastChecksumPWC = checksum;

            this.TransmissionStorage.SaveObject(obj);
        }

        /// <summary>
        /// Uploads the file content to the remote document.
        /// </summary>
        /// <returns>The SHA-1 hash of the uploaded file content.</returns>
        /// <param name="localFile">Local file.</param>
        /// <param name="doc">Remote document.</param>
        /// <param name="transmissionManager">Transmission manager.</param>
        /// <param name="transmissionEvent">File Transmission event.</param>
        /// <param name="mappedObject">Mapped object saved in <c>Storage</c></param>
        protected byte[] UploadFileWithPWC(IFileInfo localFile, ref IDocument doc, FileTransmissionEvent transmissionEvent, IMappedObject mappedObject = null) {
            byte[] checksum = null;
            IDocument docPWC = LoadRemotePWCDocument(doc, ref checksum);

            using (var file = localFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)) {
                if (checksum != null) {
                    // check PWC checksum for integration
                    using (var hashAlg = new SHA1Managed()) {
                        int bufsize = 8 * 1024;
                        byte[] buffer = new byte[bufsize];
                        for (long offset = 0; offset < docPWC.ContentStreamLength.GetValueOrDefault();) {
                            int readsize = bufsize;
                            if (readsize + offset > docPWC.ContentStreamLength.GetValueOrDefault()) {
                                readsize = (int)(docPWC.ContentStreamLength.GetValueOrDefault() - offset);
                            }

                            readsize = file.Read(buffer, 0, readsize);
                            hashAlg.TransformBlock(buffer, 0, readsize, buffer, 0);
                            offset += readsize;
                            if (readsize == 0) {
                                break;
                            }
                        }
                        hashAlg.TransformFinalBlock(new byte[0], 0, 0);
                        if (!hashAlg.Hash.SequenceEqual(checksum)) {
                            docPWC.DeleteContentStream();
                        }

                        file.Seek(0, SeekOrigin.Begin);
                    }
                }

                byte[] hash = null;
                IFileUploader uploader = FileTransmission.ContentTaskUtils.CreateUploader(this.TransmissionStorage.ChunkSize);
                transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { Started = true });
                using (var hashAlg = new SHA1Reuse()) {
                    try {
                        using (NonClosingHashStream hashstream = new NonClosingHashStream(file, hashAlg, CryptoStreamMode.Read)) {
                            int bufsize = 8 * 1024;
                            byte[] buffer = new byte[bufsize];
                            for (long offset = 0; offset < docPWC.ContentStreamLength.GetValueOrDefault(); ) {
                                int readsize = bufsize;
                                if (readsize + offset > docPWC.ContentStreamLength.GetValueOrDefault()) {
                                    readsize = (int)(docPWC.ContentStreamLength.GetValueOrDefault() - offset);
                                }

                                readsize = hashstream.Read(buffer, 0, readsize);
                                offset += readsize;
                                if (readsize == 0) {
                                    break;
                                }
                            }
                        }

                        IDocument document = doc;
                        uploader.UploadFile(docPWC, file, transmissionEvent, hashAlg, false, (byte[] checksumUpdate) => this.SaveRemotePWCDocument(localFile, document, docPWC, checksumUpdate, transmissionEvent));
                        hash = hashAlg.Hash;
                    } catch (FileTransmission.AbortException ex) {
                        hashAlg.TransformFinalBlock(new byte[0], 0, 0);
                        this.SaveRemotePWCDocument(localFile, doc, docPWC, hashAlg.Hash, transmissionEvent);
                        transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { FailedException = ex });
                        throw;
                    } catch (Exception ex) {
                        transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { FailedException = ex });
                        throw;
                    }
                }

                doc = this.Session.GetObject(docPWC.CheckIn(true, null, null, string.Empty)) as IDocument;
                doc.Refresh();   //  Refresh is required, or DotCMIS will use cached one only

                transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { Completed = true });
                return hash;
            }
        }
    }
}