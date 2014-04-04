using System;
using System.IO;
using DotCMIS.Client;
using DotCMIS.Data.Impl;
using CmisSync.Lib.Events;
using CmisSync.Lib.Streams;
using CmisSync.Lib;
using System.Security.Cryptography;

namespace CmisSync.Lib.ContentTasks
{
    /// <summary>
    /// Simple file uploader. Takes a given stream and uploads it to the server.
    /// Resuming an Upload is not supported.
    /// </summary>
    public class SimpleFileUploader : IFileUploader
    {
        private bool disposed = false;

        private object DisposeLock = new object();

        /// <summary>
        ///  Uploads the localFileStream to remoteDocument.
        /// </summary>
        /// <returns>
        ///  The new CMIS document.
        /// </returns>
        /// <param name='remoteDocument'>
        ///  Remote document where the local content should be uploaded to.
        /// </param>
        /// <param name='localFileStream'>
        ///  Local file stream.
        /// </param>
        /// <param name='TransmissionStatus'>
        ///  Transmission status where the uploader should report its uploading status.
        /// </param>
        /// <param name='hashAlg'>
        ///  Hash alg which should be used to calculate a checksum over the uploaded content.
        /// </param>
        /// <param name='overwrite'>
        ///  If true, the local content will overwrite the existing content.
        /// </param>
        /// <exception cref="CmisSync.Lib.Tasks.UploadFailedException"></exception>
        public virtual IDocument UploadFile (IDocument remoteDocument, Stream localFileStream, FileTransmissionEvent TransmissionStatus, HashAlgorithm hashAlg, bool overwrite = true)
        {
            using(NonClosingHashStream hashstream = new NonClosingHashStream(localFileStream, hashAlg, CryptoStreamMode.Read))
            using(ProgressStream progressstream = new ProgressStream(hashstream, TransmissionStatus)) {
                ContentStream contentStream = new ContentStream();
                contentStream.FileName = remoteDocument.Name;
                contentStream.MimeType = Cmis.MimeType.GetMIMEType(contentStream.FileName);
                contentStream.Stream = progressstream;
                try{
                    remoteDocument.SetContentStream(contentStream, overwrite, true);
                }catch(Exception e) {
                    throw new UploadFailedException(e, remoteDocument);
                }
            }
            hashAlg.TransformFinalBlock(new byte[0], 0, 0);
            return remoteDocument;
        }

        /// <summary>
        ///  Appends the localFileStream to the remoteDocument.
        /// </summary>
        /// <returns>
        ///  The new CMIS document.
        /// </returns>
        /// <param name='remoteDocument'>
        ///  Remote document where the local content should be appended to.
        /// </param>
        /// <param name='localFileStream'>
        ///  Local file stream.
        /// </param>
        /// <param name='TransmissionStatus'>
        ///  Transmission status where the uploader should report its appending status.
        /// </param>
        /// <param name='hashAlg'>
        ///  Hash alg which should be used to calculate a checksum over the appended content.
        /// </param>
        /// <exception cref="CmisSync.Lib.Tasks.UploadFailedException"></exception>
        public virtual IDocument AppendFile (IDocument remoteDocument, Stream localFileStream, FileTransmissionEvent TransmissionStatus, HashAlgorithm hashAlg)
        {
            using(ProgressStream progressstream = new ProgressStream(localFileStream, TransmissionStatus))
            using(CryptoStream hashstream = new CryptoStream(progressstream, hashAlg, CryptoStreamMode.Read)) {
                ContentStream contentStream = new ContentStream();
                contentStream.FileName = remoteDocument.Name;
                contentStream.MimeType = Cmis.MimeType.GetMIMEType(contentStream.FileName);
                contentStream.Stream = hashstream;
                try{
                    return remoteDocument.AppendContentStream(contentStream, true);
                }catch(Exception e) {
                    throw new UploadFailedException(e, remoteDocument);
                }
            }
        }

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            lock(DisposeLock) {
            // Check to see if Dispose has already been called.
            if(!this.disposed)
                // Note disposing has been done.
                disposed = true;
            }
        }
    }
}

