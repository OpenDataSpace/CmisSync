//-----------------------------------------------------------------------
// <copyright file="ChunkedDownloader.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.FileTransmission
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Streams;

    using DotCMIS.Client;
    using DotCMIS.Data;

    using log4net;

    /// <summary>
    /// Chunked downloader.
    /// </summary>
    public class ChunkedDownloader : IFileDownloader
    {
        private bool disposed = false;
        private object disposeLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.FileTransmission.ChunkedDownloader"/> class.
        /// </summary>
        /// <param name="chunkSize">Chunk size.</param>
        /// <param name="storage"><c>IFileTransmissionStorage</c> to persist</param>
        public ChunkedDownloader(long chunkSize = 1024 * 1024, IFileTransmissionStorage storage = null) {
            if (chunkSize <= 0) {
                throw new ArgumentException("The chunk size must be a positive number and cannot be zero or less");
            }

            this.ChunkSize = chunkSize;
            this.Storage = storage;
        }

        /// <summary>
        /// Gets the size of the chunk.
        /// </summary>
        /// <value>The size of the chunk.</value>
        public long ChunkSize { get; private set; }

        /// <summary>
        /// Gets the <c>IFileTransmissionStorage for persistence support</c>.
        /// </summary>
        public IFileTransmissionStorage Storage { get; private set; }

        /// <summary>
        /// Downloads the file and returns the SHA-1 hash of the content of the saved file
        /// </summary>
        /// <param name="remoteDocument">Remote document.</param>
        /// <param name="localFileStream">Local taget file stream.</param>
        /// <param name="transmission">Transmission status.</param>
        /// <param name="hashAlg">Hash algoritm, which should be used to calculate hash of the uploaded stream content</param>
        /// <param name="update">Not or not yet used</param>
        /// <exception cref="IOException">On any disc or network io exception</exception>
        /// <exception cref="DisposeException">If the remote object has been disposed before the dowload is finished</exception>
        /// <exception cref="AbortException">If download is aborted</exception>
        /// <exception cref="CmisException">On exceptions thrown by the CMIS Server/Client</exception>
        public void DownloadFile(
            IDocument remoteDocument,
            Stream localFileStream,
            Transmission transmission,
            HashAlgorithm hashAlg,
            UpdateChecksum update = null)
        {
            {
                byte[] buffer = new byte[8 * 1024];
                int len;
                while ((len = localFileStream.Read(buffer, 0, buffer.Length)) > 0) {
                    hashAlg.TransformBlock(buffer, 0, len, buffer, 0);
                }
            }

            long? fileLength = remoteDocument.ContentStreamLength;

            // Download content if exists
            if (fileLength > 0) {
                long offset = localFileStream.Position;
                long remainingBytes = (fileLength != null) ? (long)fileLength - offset : this.ChunkSize;
                try {
                    do {
                        offset += this.DownloadNextChunk(remoteDocument, offset, remainingBytes, transmission, localFileStream, hashAlg);
                    } while(fileLength == null);
                } catch (DotCMIS.Exceptions.CmisConstraintException) {
                }
            } else {
                transmission.Position = 0;
                transmission.Length = 0;
            }

            hashAlg.TransformFinalBlock(new byte[0], 0, 0);
        }

        /// <summary>
        /// Releases all resource used by the <see cref="CmisSync.Lib.FileTransmission.ChunkedDownloader"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="CmisSync.Lib.FileTransmission.ChunkedDownloader"/>. The <see cref="Dispose"/> method leaves the
        /// <see cref="CmisSync.Lib.FileTransmission.ChunkedDownloader"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the
        /// <see cref="CmisSync.Lib.FileTransmission.ChunkedDownloader"/> so the garbage collector can reclaim the memory
        /// that the <see cref="CmisSync.Lib.FileTransmission.ChunkedDownloader"/> was occupying.</remarks>
        public void Dispose() {
            this.Dispose(true);
        }

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be disposed.
        /// If disposing equals false, the method has been called by the
        /// runtime from inside the finalizer and you should not reference
        /// other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">If set to <c>true</c> disposing.</param>
        protected virtual void Dispose(bool disposing) {
            lock (this.disposeLock)
            {
                // Check to see if Dispose has already been called.
                if (!this.disposed) {
                    // Note disposing has been done.
                    this.disposed = true;
                }
            }
        }

        private int DownloadNextChunk(IDocument remoteDocument, long offset, long remainingBytes, Transmission transmission, Stream outputstream, HashAlgorithm hashAlg) {
            lock(this.disposeLock) {
                if (this.disposed) {
                    transmission.Status = TransmissionStatus.ABORTED;
                    throw new ObjectDisposedException(transmission.Path);
                }

                IContentStream contentStream = remoteDocument.GetContentStream(remoteDocument.ContentStreamId, offset, remainingBytes);
                transmission.Length = remoteDocument.ContentStreamLength;
                transmission.Position = offset;

                using (var remoteStream = contentStream.Stream)
                using (var forwardstream = new ForwardReadingStream(remoteStream))
                using (var offsetstream = new OffsetStream(forwardstream, offset))
                using (var progress = transmission.CreateStream(offsetstream)) {
                    byte[] buffer = new byte[8 * 1024];
                    int result = 0;
                    int len;
                    while ((len = progress.Read(buffer, 0, buffer.Length)) > 0) {
                        outputstream.Write(buffer, 0, len);
                        hashAlg.TransformBlock(buffer, 0, len, buffer, 0);
                        result += len;
                        outputstream.Flush();
                    }

                    return result;
                }
            }
        }
    }
}