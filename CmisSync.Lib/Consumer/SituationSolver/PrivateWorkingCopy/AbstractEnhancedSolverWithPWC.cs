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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Exceptions;
    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.HashAlgorithm;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;
    using CmisSync.Lib.Streams;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    using log4net;

    /// <summary>
    /// Abstract enhanced solver for Private Working Copy Usage and Support.
    /// </summary>
    public abstract class AbstractEnhancedSolverWithPWC : AbstractEnhancedSolver {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Consumer.SituationSolver.PWC.AbstractEnhancedSolverWithPWC"/> class.
        /// </summary>
        /// <param name="session">Cmis Session.</param>
        /// <param name="storage">Meta Data Storage.</param>
        /// <param name="transmissionStorage">File Transmission Storage.</param>
        public AbstractEnhancedSolverWithPWC(
            ISession session,
            IMetaDataStorage storage,
            IFileTransmissionStorage transmissionStorage) : base(session, storage, transmissionStorage) {
            if (transmissionStorage == null) {
                throw new ArgumentNullException("transmissionStorage");
            }
        }

        private IDocument CreateRemotePWCDocument(IDocument remoteDocument) {
            if (this.TransmissionStorage.GetObjectByRemoteObjectId(remoteDocument.Id) != null) {
                this.TransmissionStorage.RemoveObjectByRemoteObjectId(remoteDocument.Id);
            }

            if (string.IsNullOrEmpty(remoteDocument.VersionSeriesCheckedOutId)) {
                remoteDocument.CheckOut();
                remoteDocument.Refresh();
            }

            var remotePWCDocument = this.Session.GetObject(remoteDocument.VersionSeriesCheckedOutId) as IDocument;
            remotePWCDocument.DeleteContentStream();
            return remotePWCDocument;
        }

        private IDocument LoadRemotePWCDocument(IDocument remoteDocument, ref byte[] checksum) {
            var obj = this.TransmissionStorage.GetObjectByRemoteObjectId(remoteDocument.Id);
            if (obj == null) {
                return this.CreateRemotePWCDocument(remoteDocument);
            }

            if (obj.RemoteObjectPWCId != remoteDocument.VersionSeriesCheckedOutId) {
                return this.CreateRemotePWCDocument(remoteDocument);
            }

            var remotePWCDocument = this.Session.GetObject(remoteDocument.VersionSeriesCheckedOutId) as IDocument;
            if (remotePWCDocument == null) {
                return this.CreateRemotePWCDocument(remoteDocument);
            }

            if (remotePWCDocument.ChangeToken != obj.LastChangeTokenPWC) {
                return this.CreateRemotePWCDocument(remoteDocument);
            }

            checksum = obj.LastChecksumPWC;
            return remotePWCDocument;
        }

        private void SaveRemotePWCDocument(
            IFileInfo localFile,
            IDocument remoteDocument,
            IDocument remotePWCDocument,
            byte[] checksum,
            Transmission transmissionEvent)
        {
            if (remotePWCDocument == null) {
                return;
            }

            var obj = new FileTransmissionObject(transmissionEvent.Type, localFile, remoteDocument) {
                ChecksumAlgorithmName = "SHA-1",
                RemoteObjectPWCId = remotePWCDocument.Id,
                LastChangeTokenPWC = remotePWCDocument.ChangeToken,
                LastChecksumPWC = checksum
            };
            this.TransmissionStorage.SaveObject(obj);
        }

        /// <summary>
        /// Uploads the file content to the remote document.
        /// </summary>
        /// <returns>The SHA-1 hash of the uploaded file content.</returns>
        /// <param name="localFile">Local file.</param>
        /// <param name="doc">Remote document.</param>
        /// <param name="transmission">File transmission object.</param>
        /// <param name="mappedObject">Mapped object saved in <c>Storage</c></param>
        protected byte[] UploadFileWithPWC(IFileInfo localFile, ref IDocument doc, Transmission transmission, IMappedObject mappedObject = null) {
            if (doc == null) {
                throw new ArgumentNullException("doc");
            }

            if (transmission == null) {
                throw new ArgumentNullException("transmission");
            }

            byte[] checksum = null;
            var docPWC = this.LoadRemotePWCDocument(doc, ref checksum);

            using (var file = localFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)) {
                if (checksum != null) {
                    // check PWC checksum for integration
                    using (var hashAlg = new SHA1Managed()) {
                        int bufsize = 8 * 1024;
                        byte[] buffer = new byte[bufsize];
                        var remoteContentLength = docPWC.ContentStreamLength.GetValueOrDefault();
                        long offset = 0;
                        while (offset < remoteContentLength) {
                            int readsize = bufsize;
                            if (readsize + offset > remoteContentLength) {
                                readsize = (int)(remoteContentLength - offset);
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
                var uploader = FileTransmission.ContentTaskUtils.CreateUploader(this.TransmissionStorage.ChunkSize);
                using (var hashAlg = new SHA1Reuse()) {
                    try {
                        using (var hashstream = new NonClosingHashStream(file, hashAlg, CryptoStreamMode.Read)) {
                            int bufsize = 8 * 1024;
                            byte[] buffer = new byte[bufsize];
                            long offset = 0;
                            var remoteContentLength = docPWC.ContentStreamLength.GetValueOrDefault();
                            while ( offset < remoteContentLength) {
                                int readsize = bufsize;
                                if (readsize + offset > remoteContentLength) {
                                    readsize = (int)(remoteContentLength - offset);
                                }

                                readsize = hashstream.Read(buffer, 0, readsize);
                                offset += readsize;
                                if (readsize == 0) {
                                    break;
                                }
                            }
                        }

                        var document = doc;
                        uploader.UploadFile(
                            remoteDocument: docPWC,
                            localFileStream: file,
                            transmission: transmission,
                            hashAlg: hashAlg,
                            overwrite: false,
                            update: (byte[] checksumUpdate, long length) => this.SaveRemotePWCDocument(localFile, document, docPWC, checksumUpdate, transmission));
                        hash = hashAlg.Hash;
                    } catch (Exception ex) {
                        transmission.FailedException = ex;
                        throw;
                    }
                }

                this.TransmissionStorage.RemoveObjectByRemoteObjectId(doc.Id);

                var properties = new Dictionary<string, object>();
                properties.Add(PropertyIds.LastModificationDate, localFile.LastWriteTimeUtc);
                try {
                    doc = this.Session.GetObject(docPWC.CheckIn(true, properties, null, string.Empty)) as IDocument;

                    // Refresh is required, or DotCMIS will use cached one only
                    doc.Refresh();
                } catch (CmisConstraintException constraint) {
                    var uploadFailed = new UploadFailedException(constraint, doc);
                    transmission.FailedException = uploadFailed;
                    if (constraint.IsVirusDetectionException()) {
                        var virusException = new VirusDetectedException(constraint);
                        virusException.AffectedFiles.Add(localFile);
                        throw virusException;
                    } else {
                        throw uploadFailed;
                    }
                } catch (CmisStorageException storageException) {
                    var uploadFailed = new UploadFailedException(storageException, doc);
                    transmission.FailedException = uploadFailed;
                    if (storageException.IsVirusScannerUnavailableException()) {
                        var virusScannerUnavailable = new VirusScannerUnavailableException(storageException);
                        transmission.FailedException = virusScannerUnavailable;
                        throw virusScannerUnavailable;
                    }

                    throw uploadFailed;
                } catch (Exception ex) {
                    var uploadFailed = new UploadFailedException(ex, doc);
                    transmission.FailedException = uploadFailed;
                    throw uploadFailed;
                }

                transmission.Status = TransmissionStatus.Finished;
                return hash;
            }
        }
    }
}