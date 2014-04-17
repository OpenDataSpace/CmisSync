//-----------------------------------------------------------------------
// <copyright file="MappedObjectData.cs" company="GRAU DATA AG">
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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CmisSync.Lib.Data
{
    using System;

    /// <summary>
    /// Mapped object type.
    /// </summary>
    [Serializable]
    public enum MappedObjectType
    {
        /// <summary>
        /// The type is unkown. This should never happen, but is inserted as help for detecting not set type.
        /// </summary>
        Unkown = 0,

        /// <summary>
        /// The type is a file.
        /// </summary>
        File = 1,

        /// <summary>
        /// The typs is a folder.
        /// </summary>
        Folder = 2
    }

    /// <summary>
    /// Mapped object data to save the content of a mapped object in the MetaDataStorage.
    /// </summary>
    [Serializable]
    public class MappedObjectData
    {
        /// <summary>
        /// The parent identifier.
        /// </summary>
        public string ParentId { get; set; }

        /// <summary>
        /// The type.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public MappedObjectType Type { get; set; }

        /// <summary>
        /// The remote object identifier.
        /// </summary>
        public string RemoteObjectId { get; set; }

        /// <summary>
        /// The last changeToken of the remote object seen on server.
        /// </summary>
        public string LastChangeToken { get; set; }

        /// <summary>
        /// The last remote write time in UTC.
        /// </summary>
        public DateTime? LastRemoteWriteTimeUtc { get; set; }

        /// <summary>
        /// The last local write time in UTC.
        /// </summary>
        public DateTime? LastLocalWriteTimeUtc { get; set; }

        /// <summary>
        /// The last file content checksum.
        /// </summary>
        public byte[] LastChecksum { get; set; }

        /// <summary>
        /// The name of the checksum algorithm.
        /// </summary>
        public string ChecksumAlgorithmName { get; set; }

        /// <summary>
        /// The file/folder name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description attached to the CmisObject.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The GUID on server and or client side.
        /// </summary>
        public Guid Guid { get; set; }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to MappedObjectData return false.
            MappedObjectData p = obj as MappedObjectData;
            if (p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (Equals(this.ParentId, p.ParentId)) &&
                (this.Type.Equals(p.Type)) &&
                    (Equals(this.RemoteObjectId, p.RemoteObjectId)) &&
                    (Equals(this.LastChangeToken, p.LastChangeToken)) &&
                    (Equals(this.LastRemoteWriteTimeUtc, p.LastRemoteWriteTimeUtc)) &&
                    (Equals(this.LastLocalWriteTimeUtc, p.LastLocalWriteTimeUtc)) &&
                    (Equals(this.LastChecksum, p.LastChecksum)) &&
                    (Equals(this.ChecksumAlgorithmName, p.ChecksumAlgorithmName)) &&
                    (Equals(this.Name, p.Name)) &&
                    (Equals(this.Guid, p.Guid));
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="CmisSync.Lib.Data.MappedObjectData"/> object.
        /// </summary>
        /// <returns>
        /// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return (this.RemoteObjectId != null) ? this.RemoteObjectId.GetHashCode(): base.GetHashCode();
        }
    }
}
