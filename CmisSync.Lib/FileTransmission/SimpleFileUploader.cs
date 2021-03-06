﻿//-----------------------------------------------------------------------
// <copyright file="SimpleFileUploader.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.FileTransmission {
    using System;
    using System.IO;
    using System.Security.Cryptography;

    using CmisSync.Lib.Events;

    using DataSpace.Common.Streams;
    using DataSpace.Common.Transmissions;

    using DotCMIS.Client;
    using DotCMIS.Data.Impl;

    /// <summary>
    /// Simple file uploader. Takes a given stream and uploads it to the server.
    /// Resuming an Upload is not supported.
    /// </summary>
    public class SimpleFileUploader : IFileUploader {
        private bool disposed;

        private object disposeLock = new object();

        /// <summary>
        ///  Uploads the localFileStream to remoteDocument.
        /// </summary>
        /// <returns>The new CMIS document.</returns>
        /// <param name='remoteDocument'>Remote document where the local content should be uploaded to.</param>
        /// <param name='localFileStream'>Local file stream.</param>
        /// <param name='transmission'>Transmission status where the uploader should report its uploading status.</param>
        /// <param name='hashAlg'>Hash alg which should be used to calculate a checksum over the uploaded content.</param>
        /// <param name='overwrite'>If true, the local content will overwrite the existing content.</param>
        /// <param name="update">Is called on every chunk and returns the actual hash from beginning to this last chunk.</param>
        /// <exception cref="UploadFailedException">If upload fails</exception>
        public virtual IDocument UploadFile(
            IDocument remoteDocument,
            Stream localFileStream,
            Transmission transmission,
            HashAlgorithm hashAlg,
            bool overwrite = true,
            Action<byte[], long> update = null)
        {
            if (remoteDocument == null) {
                throw new ArgumentException("remoteDocument can not be null");
            }

            if (localFileStream == null) {
                throw new ArgumentException("localFileStream can not be null");
            }

            if (transmission == null) {
                throw new ArgumentException("status can not be null");
            }

            if (hashAlg == null) {
                throw new ArgumentException("hashAlg can not be null");
            }

            using(NonClosingHashStream hashstream = new NonClosingHashStream(localFileStream, hashAlg, CryptoStreamMode.Read))
            using(var transmissionStream = transmission.CreateStream(hashstream))
            {
                ContentStream contentStream = new ContentStream();
                contentStream.FileName = remoteDocument.Name;
                contentStream.MimeType = Cmis.MimeType.GetMIMEType(contentStream.FileName);
                contentStream.Stream = transmissionStream;
                try {
                    remoteDocument.SetContentStream(contentStream, overwrite, true);
                } catch (Exception e) {
                    throw new UploadFailedException(e, remoteDocument);
                }
            }

            hashAlg.TransformFinalBlock(new byte[0], 0, 0);
            return remoteDocument;
        }

        /// <summary>
        ///  Appends the localFileStream to the remoteDocument.
        /// </summary>
        /// <returns>The new CMIS document.</returns>
        /// <param name='remoteDocument'>Remote document where the local content should be appended to.</param>
        /// <param name='localFileStream'>Local file stream.</param>
        /// <param name='transmission'>Transmission status where the uploader should report its appending status.</param>
        /// <param name='hashAlg'>Hash alg which should be used to calculate a checksum over the appended content.</param>
        /// <exception cref="UploadFailedException">If Upload fails</exception>
        public virtual IDocument AppendFile(
            IDocument remoteDocument,
            Stream localFileStream,
            Transmission transmission,
            HashAlgorithm hashAlg)
        {
            if (transmission == null) {
                throw new ArgumentNullException("transmission");
            }

            if (remoteDocument == null) {
                throw new ArgumentNullException("remoteDocument");
            }

            using (var transmissionStream = transmission.CreateStream(localFileStream))
            using (var hashstream = new CryptoStream(transmissionStream, hashAlg, CryptoStreamMode.Read)) {
                ContentStream contentStream = new ContentStream();
                contentStream.FileName = remoteDocument.Name;
                contentStream.MimeType = Cmis.MimeType.GetMIMEType(contentStream.FileName);
                contentStream.Stream = hashstream;
                try {
                    return remoteDocument.AppendContentStream(contentStream, true);
                } catch (Exception e) {
                    throw new UploadFailedException(e, remoteDocument);
                }
            }
        }

        /// <summary>
        /// Releases all resource used by the <see cref="CmisSync.Lib.FileTransmission.SimpleFileUploader"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="CmisSync.Lib.FileTransmission.SimpleFileUploader"/>. The <see cref="Dispose"/> method leaves the
        /// <see cref="CmisSync.Lib.FileTransmission.SimpleFileUploader"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the
        /// <see cref="CmisSync.Lib.FileTransmission.SimpleFileUploader"/> so the garbage collector can reclaim the memory
        /// that the <see cref="CmisSync.Lib.FileTransmission.SimpleFileUploader"/> was occupying.</remarks>
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
            lock(this.disposeLock) {
                // Check to see if Dispose has already been called.
                if(!this.disposed) {
                    // Note disposing has been done.
                    this.disposed = true;
                }
            }
        }
    }
}