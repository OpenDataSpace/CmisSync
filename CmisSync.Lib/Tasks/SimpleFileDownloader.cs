using System;
using DotCMIS.Client;
using System.Security.Cryptography;
using System.IO;
using CmisSync.Lib.Streams;
using CmisSync.Lib.Events;

namespace CmisSync.Lib.Tasks
{
    public class SimpleFileDownloader : FileDownloader
    {
        public byte[] DownloadFile (IDocument remoteDocument, Stream localFileStream, FileTransmissionEvent TransmissionStatus)
        {
            long? fileLength = remoteDocument.ContentStreamLength;
            DotCMIS.Data.IContentStream contentStream = remoteDocument.GetContentStream ();

            // Skip downloading empty content, just go on with an empty file
            if (null == fileLength || fileLength == 0 || contentStream == null) {
                using (SHA1 sha = new SHA1CryptoServiceProvider()) {
                    return sha.ComputeHash (new byte[0]);
                }
            }
            using (SHA1 hashAlg = new SHA1Managed())
            using (CryptoStream hashstream = new CryptoStream(localFileStream, hashAlg, CryptoStreamMode.Write))
            using (ProgressStream progressStream = new ProgressStream(hashstream, TransmissionStatus))
            using (contentStream.Stream) {
                byte[] buffer = new byte[8 * 1024];
                int len;
                while ((len = contentStream.Stream.Read(buffer, 0, buffer.Length)) > 0) {
                    progressStream.Write (buffer, 0, len);
                }
                return hashAlg.Hash;
            }
        }
    }
}
