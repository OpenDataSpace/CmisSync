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

namespace CmisSync.Lib.FileTransmission {
    using System;
    using System.IO;
    using System.Security.Cryptography;

    using CmisSync.Lib.Events;

    using DotCMIS.Client;

    /// <summary>
    /// File downloader interface.
    /// </summary>
    public interface IFileDownloader : IDisposable {
        /// <summary>
        /// Downloads the file and returns the SHA-1 hash of the content of the saved file
        /// </summary>
        /// <param name='remoteDocument'>
        /// Remote document.
        /// </param>
        /// <param name='localFileStream'>
        /// Local taget file stream.
        /// </param>
        /// <param name='transmissionStatus'>
        /// Transmission status.
        /// </param>
        /// <param name='hashAlg'>
        /// Hash algoritm, which should be used to calculate hash of the uploaded stream content
        /// </param>
        /// <exception cref="IOException">On any disc or network io exception</exception>
        /// <exception cref="DisposeException">If the remote object has been disposed before the dowload is finished</exception>
        /// <exception cref="AbortException">If download is aborted</exception>
        /// <exception cref="CmisException">On exceptions thrown by the CMIS Server/Client</exception>
        void DownloadFile(IDocument remoteDocument, Stream localFileStream, Transmission transmissionStatus, HashAlgorithm hashAlg);
    }
}