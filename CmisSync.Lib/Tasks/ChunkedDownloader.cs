using System;
using DotCMIS.Client;
using DotCMIS.Data;
using System.Security.Cryptography;
using System.IO;
using CmisSync.Lib.Streams;
using CmisSync.Lib.Events;
using log4net;

namespace CmisSync.Lib.Tasks
{
    public class ChunkedDownloader : FileDownloader
    {
        private bool disposed = false;
        private object DisposeLock = new object ();
        private long chunkSize;

        public long ChunkSize { get { return this.chunkSize; } }

        public ChunkedDownloader (long ChunkSize = 1024 * 1024)
        {
            if (ChunkSize <= 0)
                throw new ArgumentException ("The chunk size must be a positive number and cannot be zero or less");
            this.chunkSize = ChunkSize;
        }

        private int DownloadNextChunk (IDocument remoteDocument, long offset, long remainingBytes, FileTransmissionEvent TransmissionStatus, Stream outputstream, HashAlgorithm hashAlg)
        {
            lock (DisposeLock) {
                if (disposed) {
                    throw new ObjectDisposedException ("Downloading");
                }

                IContentStream contentStream = remoteDocument.GetContentStream (remoteDocument.ContentStreamId, offset, remainingBytes);
                TransmissionStatus.ReportProgress (new TransmissionProgressEventArgs () {
                    Length = remoteDocument.ContentStreamLength,
                    ActualPosition = offset,
                    Resumed = offset > 0
                }
                );

                using (Stream remoteStream = contentStream.Stream)
                using (OffsetStream offsetstream = new OffsetStream(remoteStream, offset))
                using (ProgressStream progress = new ProgressStream(offsetstream, TransmissionStatus)) {
                    byte[] buffer = new byte[8 * 1024];
                    int result = 0;
                    int len;
                    while ((len = progress.Read (buffer, 0, buffer.Length)) > 0) {
                        outputstream.Write (buffer, 0, len);
                        hashAlg.TransformBlock (buffer, 0, len, buffer, 0);
                        result += len;
                    }
                    return result;
                }
            }
        }

        public void DownloadFile (IDocument remoteDocument, Stream localFileStream, FileTransmissionEvent TransmissionStatus, HashAlgorithm hashAlg)
        {
            {
                byte[] buffer = new byte[8 * 1024];
                int len;
                while ((len = localFileStream.Read(buffer, 0, buffer.Length)) > 0) {
                    hashAlg.TransformBlock (buffer, 0, len, buffer, 0);
                }
            }

            long? fileLength = remoteDocument.ContentStreamLength;
            // Skip downloading empty content, just go on with an empty file
            if (null == fileLength || fileLength == 0) {
                hashAlg.TransformFinalBlock (new byte[0], 0, 0);
                return;
            }
            long offset = localFileStream.Position;
            long remainingBytes = (fileLength != null) ? (long)fileLength - offset : chunkSize;
            try {
                do {
                    offset += DownloadNextChunk (remoteDocument, offset, remainingBytes, TransmissionStatus, localFileStream, hashAlg);
                } while(fileLength == null);
            } catch (DotCMIS.Exceptions.CmisConstraintException) {
            }
            hashAlg.TransformFinalBlock (new byte[0], 0, 0);
        }

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose ()
        {
            Dispose (true);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose (bool disposing)
        {
            lock (DisposeLock) {
                // Check to see if Dispose has already been called.
                if (!this.disposed)
                // Note disposing has been done.
                    disposed = true;
            }
        }
    }
}

