//-----------------------------------------------------------------------
// <copyright file="IMappedObject.cs" company="GRAU DATA AG">
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
    using System.ComponentModel;
    using System.IO;
    using System.Text;

    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    /// <summary>
    /// Interface for a mapped object. A mapped object is a metadata representation of a local file, corresponding to a remote file.
    /// </summary>
    public interface IMappedObject
    {
        /// <summary>
        /// Gets or sets the CMIS remote object identifier.
        /// </summary>
        /// <value>The remote object identifier.</value>
        string RemoteObjectId { get; set; }

        /// <summary>
        /// Gets or sets the CMIS parent identifier.
        /// </summary>
        /// <value>The parent identifier.</value>
        string ParentId { get; set; }

        /// <summary>
        /// Gets or sets the last change token of last action made on CMIS server.
        /// </summary>
        /// <value>The last change token.</value>
        string LastChangeToken { get; set; }

        /// <summary>
        /// Gets or sets the last remote write time in UTC.
        /// </summary>
        /// <value>The last remote write time.</value>
        DateTime? LastRemoteWriteTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the last local write time in UTC.
        /// </summary>
        /// <value>The last local write time.</value>
        DateTime? LastLocalWriteTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the last file content checksum.
        /// </summary>
        /// <value>The last checksum.</value>
        byte[] LastChecksum { get; set; }

        /// <summary>
        /// Gets or sets the name of the checksum algorithm.
        /// </summary>
        /// <value>The name of the checksum algorithm.</value>
        string ChecksumAlgorithmName { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        string Description { get; set; }

        /// <summary>
        /// Gets or sets the GUID.
        /// </summary>
        /// <value>The GUID.</value>
        Guid Guid { get; set; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        MappedObjectType Type { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CmisSync.Lib.Data.IMappedObject"/> is ignored.
        /// </summary>
        /// <value><c>true</c> if ignored; otherwise, <c>false</c>.</value>
        bool Ignored { get; set; }

        /// <summary>
        /// Gets or sets the last size of the file or the last size of the folder content. Default value is -1.
        /// </summary>
        /// <value>
        /// The last size of the file or folder content.
        /// </value>
        long LastContentSize { get; set; }
    }
}