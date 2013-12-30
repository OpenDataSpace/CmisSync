using System;
using System.IO;
using DotCMIS.Client;
using CmisSync.Lib.Events;
using System.Security.Cryptography;

namespace CmisSync.Lib
{
    namespace Tasks
    {
        public interface IFileUploader : IDisposable
        {
            IDocument UploadFile (IDocument remoteDocument, Stream localFileStream, FileTransmissionEvent TransmissionStatus, HashAlgorithm hashAlg, bool overwrite = true);
            IDocument AppendFile (IDocument remoteDocument, Stream localFileStream, FileTransmissionEvent TransmissionStatus, HashAlgorithm hashAlg);
        }
    }
}

