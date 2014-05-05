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

