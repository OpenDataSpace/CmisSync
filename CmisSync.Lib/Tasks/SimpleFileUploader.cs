using System;
using System.IO;
using DotCMIS.Client;
using CmisSync.Lib.Events;
using System.Security.Cryptography;

namespace CmisSync.Lib.Tasks
{
    public class SimpleFileUploader : IFileUploader
    {
        public SimpleFileUploader ()
        {
        }

        public void UploadFile (IDocument remoteDocument, Stream localFileStream, FileTransmissionEvent TransmissionStatus, HashAlgorithm hashAlg)
        {

        }
    }
}

