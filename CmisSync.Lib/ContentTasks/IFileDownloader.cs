//-----------------------------------------------------------------------
// <copyright file="IFileDownloader.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------
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

