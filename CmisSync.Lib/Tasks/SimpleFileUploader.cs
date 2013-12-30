using System;
using System.IO;
using DotCMIS.Client;
using DotCMIS.Data.Impl;
using CmisSync.Lib.Events;
using CmisSync.Lib.Streams;
using CmisSync.Lib;
using System.Security.Cryptography;

namespace CmisSync.Lib.Tasks
{
    public class SimpleFileUploader : IFileUploader
    {
        private bool disposed = false;

        private object DisposeLock = new object();

        public virtual IDocument UploadFile (IDocument remoteDocument, Stream localFileStream, FileTransmissionEvent TransmissionStatus, HashAlgorithm hashAlg, bool overwrite = true)
        {
            using(ProgressStream progressstream = new ProgressStream(localFileStream, TransmissionStatus))
            using(CryptoStream hashstream = new CryptoStream(progressstream, hashAlg, CryptoStreamMode.Read)) {
                ContentStream contentStream = new ContentStream();
                contentStream.FileName = remoteDocument.Name;
                contentStream.MimeType = Cmis.MimeType.GetMIMEType(contentStream.FileName);
                contentStream.Stream = hashstream;
                return remoteDocument.SetContentStream(contentStream, overwrite);
            }
        }

        public virtual IDocument AppendFile (IDocument remoteDocument, Stream localFileStream, FileTransmissionEvent TransmissionStatus, HashAlgorithm hashAlg)
        {
            using(ProgressStream progressstream = new ProgressStream(localFileStream, TransmissionStatus))
            using(CryptoStream hashstream = new CryptoStream(progressstream, hashAlg, CryptoStreamMode.Read)) {
                ContentStream contentStream = new ContentStream();
                contentStream.FileName = remoteDocument.Name;
                contentStream.MimeType = Cmis.MimeType.GetMIMEType(contentStream.FileName);
                contentStream.Stream = hashstream;
                return remoteDocument.AppendContentStream(contentStream, true);
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

