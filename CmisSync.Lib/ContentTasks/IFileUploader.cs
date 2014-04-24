//-----------------------------------------------------------------------
// <copyright file="IFileUploader.cs" company="GRAU DATA AG">
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
using System;
using System.Runtime.Serialization;
using System.IO;
using DotCMIS.Client;
using CmisSync.Lib.Events;
using System.Security.Cryptography;

namespace CmisSync.Lib.ContentTasks
{
    /// <summary>
    /// I file Upload Module must implement this interface.
    /// </summary>
    public interface IFileUploader : IDisposable
    {
        /// <summary>
        /// Uploads the localFileStream to remoteDocument.
        /// </summary>
        /// <returns>
        /// The new CMIS document.
        /// </returns>
        /// <param name='remoteDocument'>
        /// Remote document where the local content should be uploaded to.
        /// </param>
        /// <param name='localFileStream'>
        /// Local file stream.
        /// </param>
        /// <param name='TransmissionStatus'>
        /// Transmission status where the uploader should report its uploading status.
        /// </param>
        /// <param name='hashAlg'>
        /// Hash alg which should be used to calculate a checksum over the uploaded content.
        /// </param>
        /// <param name='overwrite'>
        /// If true, the local content will overwrite the existing content.
        /// </param>
        IDocument UploadFile (IDocument remoteDocument, Stream localFileStream, FileTransmissionEvent TransmissionStatus, HashAlgorithm hashAlg, bool overwrite = true);
        /// <summary>
        /// Appends the localFileStream to the remoteDocument.
        /// </summary>
        /// <returns>
        /// The new CMIS document.
        /// </returns>
        /// <param name='remoteDocument'>
        /// Remote document where the local content should be appended to.
        /// </param>
        /// <param name='localFileStream'>
        /// Local file stream.
        /// </param>
        /// <param name='TransmissionStatus'>
        /// Transmission status where the uploader should report its appending status.
        /// </param>
        /// <param name='hashAlg'>
        /// Hash alg which should be used to calculate a checksum over the appended content.
        /// </param>
        IDocument AppendFile (IDocument remoteDocument, Stream localFileStream, FileTransmissionEvent TransmissionStatus, HashAlgorithm hashAlg);
    }

    /// <summary>
    /// Upload failed exception.
    /// </summary>
    [Serializable]
    public class UploadFailedException : Exception
    {
        private IDocument doc;
        /// <summary>
        /// Gets the last successful uploaded document state.
        /// </summary>
        /// <value>
        /// The last successful uploaded document state.
        /// </value>
        public IDocument LastSuccessfulDocument { get { return this.doc; } }
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Tasks.UploadFailedException"/> class.
        /// </summary>
        /// <param name='inner'>
        /// Inner exception cause the upload failure.
        /// </param>
        /// <param name='lastSuccessfulDocumentState'>
        /// Last successful uploaded document state.
        /// </param>
        public UploadFailedException (Exception inner, IDocument lastSuccessfulDocumentState) : base("Upload Failed", inner)
        {
            doc = lastSuccessfulDocumentState;
        }

        public UploadFailedException () : this("Upload Failed")
        {
        }

        public UploadFailedException (string message) : base(message)
        {
            doc = null;
        }

        public UploadFailedException (string message, Exception innerException) : base(message, innerException)
        {
            doc = null;
        }

        protected UploadFailedException (SerializationInfo info, StreamingContext context) : base (info, context)
        {
            doc = null;
        }
    }
}

