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
    /// Chunked uploader takes a file and splits the upload into chunks.
    /// Resuming a failed upload is possible.
    /// </summary>
    public class ChunkedUploader : SimpleFileUploader
    {
        private long chunkSize;

        public long ChunkSize { get { return this.chunkSize; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Tasks.ChunkedUploader"/> class.
        /// </summary>
        /// <param name='ChunkSize'>
        /// Chunk size.
        /// </param>
        public ChunkedUploader (long ChunkSize = 1024 * 1024)
        {
            if (ChunkSize <= 0)
                throw new ArgumentException ("The chunk size must be a positive number and cannot be zero or less");
            this.chunkSize = ChunkSize;
        }

        /// <summary>
        ///  Uploads the file.
        ///  Resumes an upload if the given localFileStream.Position is larger than zero.
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
        /// <exception cref="CmisSync.Lib.Tasks.UploadFailedException">
        /// Contains the last successful remote document state. This is needed for continue a failed upload.
        /// </exception>
        public override IDocument UploadFile (IDocument remoteDocument, Stream localFileStream, FileTransmissionEvent TransmissionStatus, HashAlgorithm hashAlg, bool overwrite = true)
        {
            IDocument result = remoteDocument;
            for (long offset = localFileStream.Position; offset < localFileStream.Length; offset += ChunkSize)
            {
                bool isFirstChunk = (offset == 0);
                bool isLastChunk = (offset + ChunkSize) >= localFileStream.Length;
                using (NonClosingHashStream hashstream = new NonClosingHashStream(localFileStream, hashAlg, CryptoStreamMode.Read))
                using (ChunkedStream chunkstream = new ChunkedStream(hashstream, ChunkSize))
                using (OffsetStream offsetstream = new OffsetStream(chunkstream, offset))
                using (ProgressStream progressstream = new ProgressStream(offsetstream, TransmissionStatus))
                {
                    TransmissionStatus.Status.Length = localFileStream.Length;
                    TransmissionStatus.Status.ActualPosition = offset;
                    chunkstream.ChunkPosition = offset;

                    ContentStream contentStream = new ContentStream();
                    contentStream.FileName = remoteDocument.Name;
                    contentStream.MimeType = Cmis.MimeType.GetMIMEType(remoteDocument.Name);
                    if (isLastChunk)
                        contentStream.Length = localFileStream.Length - offset;
                    else
                        contentStream.Length = ChunkSize;
                    contentStream.Stream = progressstream;
                    try{
                        if(isFirstChunk && result.ContentStreamId != null && overwrite)
                            result.DeleteContentStream(true);
                        result.AppendContentStream(contentStream, isLastChunk, true);
                    }catch(Exception e) {
                        throw new UploadFailedException(e, result);
                    }
                }
            }
            hashAlg.TransformFinalBlock(new byte[0], 0, 0);
            return result;
        }


        // TODO implementation
        public override IDocument AppendFile (IDocument remoteDocument, Stream localFileStream, FileTransmissionEvent TransmissionStatus, HashAlgorithm hashAlg)
        {
            throw new NotImplementedException();
            /*
            IDocument result = remoteDocument;
            for (long offset = localFileStream.Position; offset < localFileStream.Length; offset += ChunkSize)
            {
                bool isFirstChunk = (offset == 0);
                bool isLastChunk = (offset + ChunkSize) >= localFileStream.Length;
                using (NonClosingHashStream hashstream = new NonClosingHashStream(localFileStream, hashAlg, CryptoStreamMode.Read))
                using (ChunkedStream chunkstream = new ChunkedStream(hashstream, ChunkSize))
                using (OffsetStream offsetstream = new OffsetStream(chunkstream, offset))
                using (ProgressStream progressstream = new ProgressStream(offsetstream, TransmissionStatus))
                {
                    chunkstream.ChunkPosition = offset;

                    ContentStream contentStream = new ContentStream();
                    contentStream.FileName = remoteDocument.Name;
                    contentStream.MimeType = Cmis.MimeType.GetMIMEType(remoteDocument.Name);
                    if (isLastChunk)
                        contentStream.Length = localFileStream.Length - offset;
                    else
                        contentStream.Length = ChunkSize;
                    contentStream.Stream = chunkstream;
                    if(isFirstChunk && result.ContentStreamId != null)
                        result.DeleteContentStream();
                    result = result.AppendContentStream(contentStream, isLastChunk);
                }
            }
            hashAlg.TransformFinalBlock(new byte[0], 0, 0);
            return result;*/
        }
    }
}

