using System;
using System.IO;
using System.Security.Cryptography;

namespace CmisSync.Lib.ContentTasks
{
    public static class ContentTaskUtils {
        public static IFileUploader CreateUploader (long chunkSize = 0)
        {
            if (chunkSize > 0)
                return new ChunkedUploader(chunkSize);
            return new SimpleFileUploader();
        }

        public static void PrepareResume(long successfulLength, System.IO.Stream successfulPart, HashAlgorithm hashAlg)
        {
            byte[] buffer = new byte[4096];
            int pos = 0;
            while(pos < successfulLength)
            {
                int l = successfulPart.Read(buffer, 0, (int) Math.Min(buffer.Length, successfulLength - pos));
                if(l<=0) {
                    throw new IOException(String.Format("File stream is shorter ({0}) than the given length {1}", pos, successfulLength));
                }
                hashAlg.TransformBlock (buffer, 0, l, buffer, 0);
                pos += l;
            }
        }

        public static IFileDownloader CreateDownloader (long chunkSize = 0)
        {
            if(chunkSize > 0)
                return new ChunkedDownloader(chunkSize);
            return new SimpleFileDownloader();
        }
    }
}