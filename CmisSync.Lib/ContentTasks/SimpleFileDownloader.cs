using System;
using DotCMIS.Client;
using System.Security.Cryptography;
using System.IO;
using CmisSync.Lib.Streams;
using CmisSync.Lib.Events;

namespace CmisSync.Lib.ContentTasks
{
    public class SimpleFileDownloader : IFileDownloader
    {
        private bool disposed = false;

        private object DisposeLock = new object();

        public void DownloadFile (IDocument remoteDocument, Stream localFileStream, FileTransmissionEvent TransmissionStatus, HashAlgorithm hashAlg)
        {
            long? fileLength = remoteDocument.ContentStreamLength;
            DotCMIS.Data.IContentStream contentStream = remoteDocument.GetContentStream ();

            // Skip downloading empty content, just go on with an empty file
            if (null == fileLength || fileLength == 0 || contentStream == null) {
                hashAlg.TransformFinalBlock(new byte[0], 0, 0);
                return;
            }
            using (ProgressStream progressStream = new ProgressStream(localFileStream, TransmissionStatus))
            using (CryptoStream hashstream = new CryptoStream(progressStream, hashAlg, CryptoStreamMode.Write))
            using (Stream remoteStream = contentStream.Stream) {
                TransmissionStatus.ReportProgress (new TransmissionProgressEventArgs () {
                    Length = remoteDocument.ContentStreamLength,
                    ActualPosition = 0
                }
                );
                byte[] buffer = new byte[8 * 1024];
                int len;
                while ((len = remoteStream.Read(buffer, 0, buffer.Length)) > 0) {
                    lock(DisposeLock){
                        if(this.disposed) {
                            TransmissionStatus.ReportProgress(new TransmissionProgressEventArgs(){Aborted = true});
                            throw new ObjectDisposedException(TransmissionStatus.Path);
                        }
                        hashstream.Write (buffer, 0, len);
                        hashstream.Flush();
                    }
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
