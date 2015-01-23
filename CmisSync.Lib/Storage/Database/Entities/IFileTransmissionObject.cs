//-----------------------------------------------------------------------
// <copyright file="IFileTransmissionObject.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Storage.Database.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using CmisSync.Lib.Events;

    /// <summary>
    /// Interface for a file transmission object. A file transmission object is a record for a upload/download file transmission.
    /// </summary>
    public interface IFileTransmissionObject
    {
        /// <summary>
        /// Gets the type
        /// </summary>
        FileTransmissionType Type { get; }

        /// <summary>
        /// Gets the local file path 
        /// </summary>
        /// <value>The local file path</value>
        string LocalPath { get; }

        /// <summary>
        /// Gets or sets the last size of the file
        /// </summary>
        long LastContentSize { get; set; }

        /// <summary>
        /// Gets or sets the last file content checksum for local file
        /// </summary>
        /// <value>The last file content checksum for local file</value>
        byte[] LastChecksum { get; set; }

        /// <summary>
        /// Gets or sets the name of the checksum algorithm for <c>LastCheckSum</c>
        /// </summary>
        /// <value>The name of the checksum algorithm for <c>LastCheckSum</c></value>
        string ChecksumAlgorithmName { get; set; }

        /// <summary>
        /// Gets or sets the last local write time in UTC
        /// </summary>
        /// <value>The last local write time in UTC</value>
        DateTime? LastLocalWriteTimeUtc { get; set; }

        /// <summary>
        /// Gets the CMIS remote object identifier
        /// </summary>
        /// <value>The CMIS remote object identifier</value>
        string RemoteObjectId { get; }

        /// <summary>
        /// Gets the CMIS remote object private working copy identifier
        /// </summary>
        string RemoteObjectPWCId { get; }

        /// <summary>
        /// Gets or sets the last change token of last action make on CMIS server
        /// </summary>
        /// <value>The last change token of last action make on CMIS server</value>
        string LastChangeToken { get; set; }

        /// <summary>
        /// Gets or sets the last remote write time in UTC
        /// </summary>
        /// <value>The last remote write time in UTC</value>
        DateTime? LastRemoteWriteTimeUtc { get; set; }
    }
}
