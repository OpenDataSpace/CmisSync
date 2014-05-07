//-----------------------------------------------------------------------
// <copyright file="MappedObject.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Data
{
    using System;
    using System.ComponentModel;

    using DotCMIS.Client;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

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
    public class MappedObject : IMappedObject
    {
        /// <summary>
        /// The extended attribute key.
        /// </summary>
        public static readonly string ExtendedAttributeKey = "DSS-UUID";

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Data.MappedObject"/> class.
        /// </summary>
        [Obsolete("Must not be used manually. This constructor should be used for serialization only.", true)]
        public MappedObject()
        {
            this.LastContentSize = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Data.MappedObject"/> class.
        /// </summary>
        /// <param name="name">Name of the Directory/Folder.</param>
        /// <param name="remoteId">Remote identifier.</param>
        /// <param name="type">Object type.</param>
        /// <param name="parentId">Parent identifier.</param>
        /// <param name="changeToken">Change token.</param>
        /// <param name="contentSize">Size of the content. Only exists on Documents.</param>
        public MappedObject(string name, string remoteId, MappedObjectType type, string parentId, string changeToken, long contentSize = -1)
        {
            if(string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("Given name is null or empty");
            }

            if(string.IsNullOrEmpty(remoteId))
            {
                throw new ArgumentNullException("Given remote ID is null");
            }

            if(type == MappedObjectType.Unkown)
            {
                throw new ArgumentException("Given type is unknown but must be set to a known type");
            }
            else
            {
                this.Type = type;
            }

            this.Name = name;
            this.RemoteObjectId = remoteId;
            this.ParentId = parentId;
            this.LastChangeToken = changeToken;
            this.LastContentSize = contentSize;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Data.MappedObject"/> class.
        /// </summary>
        /// <param name='data'>
        /// Data to copy.
        /// </param>
        public MappedObject(MappedObject data)
        {
            if(data != null)
            {
                this.ParentId = data.ParentId;
                this.Description = data.Description;
                this.ChecksumAlgorithmName = data.ChecksumAlgorithmName;
                this.Guid = data.Guid;
                this.LastChangeToken = data.LastChangeToken;
                this.LastLocalWriteTimeUtc = data.LastLocalWriteTimeUtc;
                this.LastRemoteWriteTimeUtc = data.LastRemoteWriteTimeUtc;
                this.Name = data.Name;
                this.RemoteObjectId = data.RemoteObjectId;
                this.Type = data.Type;
                this.LastContentSize = data.LastContentSize;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Data.MappedObject"/> class.
        /// </summary>
        /// <param name='remoteFolder'>
        /// A IFolder fetched via cmis.
        /// </param>
        public MappedObject(IFolder remoteFolder)
        {
            this.RemoteObjectId = remoteFolder.Id;
            this.ParentId = remoteFolder.ParentId;
            this.LastChangeToken = remoteFolder.ChangeToken;
            this.Name = remoteFolder.Name;
            this.Type = MappedObjectType.Folder;
            this.LastRemoteWriteTimeUtc = remoteFolder.LastModificationDate;
        }

        /// <summary>
        /// Gets or sets the parent identifier.
        /// </summary>
        /// <value>
        /// The parent identifier.
        /// </value>
        public string ParentId { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        [JsonConverter(typeof(StringEnumConverter))]
        public MappedObjectType Type { get; set; }

        /// <summary>
        /// Gets or sets the remote object identifier.
        /// </summary>
        /// <value>
        /// The remote object identifier.
        /// </value>
        public string RemoteObjectId { get; set; }

        /// <summary>
        /// Gets or sets the last changeToken of the remote object seen on server.
        /// </summary>
        /// <value>
        /// The last change token.
        /// </value>
        public string LastChangeToken { get; set; }

        /// <summary>
        /// Gets or sets the last remote write time in UTC.
        /// </summary>
        /// <value>
        /// The last remote write time UTC.
        /// </value>
        public DateTime? LastRemoteWriteTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the lase local write time in UTC.
        /// </summary>
        /// <value>
        /// The last local write time UTC.
        /// </value>
        public DateTime? LastLocalWriteTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the last file content checksum.
        /// </summary>
        /// <value>
        /// The last file content checksum.
        /// </value>
        public byte[] LastChecksum { get; set; }

        /// <summary>
        /// Gets or sets the name of the checksum algorithm.
        /// </summary>
        /// <value>
        /// The name of the checksum algorithm.
        /// </value>
        public string ChecksumAlgorithmName { get; set; }

        /// <summary>
        /// Gets or sets the file/folder name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description attached to the CmisObject.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the GUID on server and or client side.
        /// </summary>
        /// <value>
        /// The GUID.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the last size of the file or the last size of the folder content. Default value is -1.
        /// </summary>
        /// <value>
        /// The last size of the file or folder content.
        /// </value>
        [DefaultValue(-1)]
        public long LastContentSize { get; set; }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="CmisSync.Lib.Data.MappedObjectData"/>.
        /// </summary>
        /// <param name='obj'>
        /// The <see cref="System.Object"/> to compare with the current <see cref="CmisSync.Lib.Data.MappedObjectData"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="CmisSync.Lib.Data.MappedObject"/>; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to MappedObjectData return false.
            MappedObject p = obj as MappedObject;
            if (p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return object.Equals(this.ParentId, p.ParentId) &&
                this.Type.Equals(p.Type) &&
                    object.Equals(this.RemoteObjectId, p.RemoteObjectId) &&
                    object.Equals(this.LastChangeToken, p.LastChangeToken) &&
                    object.Equals(this.LastRemoteWriteTimeUtc, p.LastRemoteWriteTimeUtc) &&
                    object.Equals(this.LastLocalWriteTimeUtc, p.LastLocalWriteTimeUtc) &&
                    object.Equals(this.LastChecksum, p.LastChecksum) &&
                    object.Equals(this.ChecksumAlgorithmName, p.ChecksumAlgorithmName) &&
                    object.Equals(this.Name, p.Name) &&
                    object.Equals(this.Guid, p.Guid) &&
                    object.Equals(this.LastContentSize, p.LastContentSize);
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="CmisSync.Lib.Data.MappedObject"/> object.
        /// </summary>
        /// <returns>
        /// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return (this.RemoteObjectId != null) ? this.RemoteObjectId.GetHashCode() : base.GetHashCode();
        }
    }
}
