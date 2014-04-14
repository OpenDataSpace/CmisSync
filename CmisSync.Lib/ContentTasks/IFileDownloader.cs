using System;
using System.IO;
using DotCMIS.Client;
using CmisSync.Lib.Events;
using System.Security.Cryptography;

namespace CmisSync.Lib.ContentTasks
{
    public interface IFileDownloader : IDisposable
    {
        /// <summary>
        /// Downloads the file and returns the SHA1 hash of the content of the saved file
        /// </summary>
        /// <returns>
        /// SHA1 Hash of the file content
        /// </returns>
        /// <param name='remoteDocument'>
        /// Remote document.
        /// </param>
        /// <param name='localFileStream'>
        /// Local taget file stream.
        /// </param>
        /// <param name='TransmissionStatus'>
        /// Transmission status.
        /// </param>
        /// <exception cref="IOException">On any disc or network io exception</exception>
        /// <exception cref="DisposeException">If the remote object has been disposed before the dowload is finished</exception>
        /// <exception cref="AbortException">If download is aborted</exception>
        /// <exception cref="CmisException">On exceptions thrown by the CMIS Server/Client</exception>
        void DownloadFile (IDocument remoteDocument, Stream localFileStream, FileTransmissionEvent TransmissionStatus, HashAlgorithm hashAlg);
    }
}

