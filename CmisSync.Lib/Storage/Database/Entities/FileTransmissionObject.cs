//-----------------------------------------------------------------------
// <copyright file="FileTransmissionObject.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Storage.Database.Entities {
    using System;
    using System.IO;
    using System.Linq;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    /// <summary>
    /// Implementation of <c>IFileTransmissionObject</c>
    /// </summary>
    [Serializable]
    public class FileTransmissionObject : IFileTransmissionObject {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="CmisSync.Lib.Storage.Database.Entities.FileTransmissionObject"/> class.
        /// </summary>
        /// <param name="type">Type of transmission.</param>
        /// <param name="localFile">Local file.</param>
        /// <param name="remoteFile">Remote file.</param>
        public FileTransmissionObject(FileTransmissionType type, IFileInfo localFile, IDocument remoteFile) {
            if (localFile == null) {
                throw new ArgumentNullException("localFile");
            }

            if (!localFile.Exists) {
                throw new ArgumentException(string.Format("'{0} file does not exist", localFile.FullName), "localFile");
            }

            if (remoteFile == null) {
                throw new ArgumentNullException("remoteFile");
            }

            if (remoteFile.Id == null) {
                throw new ArgumentNullException("remoteFile.Id");
            }

            if (string.IsNullOrEmpty(remoteFile.Id)) {
                throw new ArgumentException("empty string", "remoteFile.Id");
            }

            this.Type = type;
            this.LocalPath = localFile.FullName;
            this.LastContentSize = localFile.Length;
            this.LastLocalWriteTimeUtc = localFile.LastWriteTimeUtc;
            this.RemoteObjectId = remoteFile.Id;
            this.LastChangeToken = remoteFile.ChangeToken;
            this.LastRemoteWriteTimeUtc = remoteFile.LastModificationDate;
            if (this.LastRemoteWriteTimeUtc != null) {
                this.LastRemoteWriteTimeUtc = this.LastRemoteWriteTimeUtc.GetValueOrDefault().ToUniversalTime();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTransmissionObject"/> class.
        /// </summary>
        [Obsolete("Must not be used manually. This constructor should be used for serialization only.", true)]
        public FileTransmissionObject() {
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="CmisSync.Lib.Storage.Database.Entities.FileTransmissionObject"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="CmisSync.Lib.Storage.Database.Entities.FileTransmissionObject"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="CmisSync.Lib.Storage.Database.Entities.FileTransmissionObject"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }

            if (this == obj) {
                return true;
            }

            // If parameter cannot be casted to FileTransmissionObject return false.
            FileTransmissionObject o = obj as FileTransmissionObject;
            if (o == null) {
                return false;
            }

            return this.Type.Equals(o.Type) &&
                object.Equals(this.LocalPath, o.LocalPath) &&
                    ((this.LastChecksum == null && o.LastChecksum == null) || (this.LastChecksum != null && o.LastChecksum != null && this.LastChecksum.SequenceEqual(o.LastChecksum))) &&
                    object.Equals(this.ChecksumAlgorithmName, o.ChecksumAlgorithmName) &&
                    object.Equals(this.LastLocalWriteTimeUtc, o.LastLocalWriteTimeUtc) &&
                    object.Equals(this.RemoteObjectId, o.RemoteObjectId) &&
                    object.Equals(this.LastChangeToken, o.LastChangeToken) &&
                    object.Equals(this.LastRemoteWriteTimeUtc, o.LastRemoteWriteTimeUtc);
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="CmisSync.Lib.Storage.Database.Entities.FileTransmissionObject"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode() {
            int localPath = (this.LocalPath ?? string.Empty).GetHashCode();
            int checksumName = (this.ChecksumAlgorithmName ?? string.Empty).GetHashCode();
            int lastlocalwritetime = this.LastLocalWriteTimeUtc.GetValueOrDefault().GetHashCode();
            int remoteId = (this.RemoteObjectId ?? string.Empty).GetHashCode();
            int changeToken = (this.LastChangeToken ?? string.Empty).GetHashCode();
            int lastremotewritetime = this.LastRemoteWriteTimeUtc.GetValueOrDefault().GetHashCode();
            return localPath ^ checksumName ^ lastlocalwritetime ^ remoteId ^ changeToken ^ lastremotewritetime;
        }

        /// <summary>
        /// Gets or sets the type
        /// </summary>
        /// <value>The type.</value>
        public FileTransmissionType Type { get; set; }

        /// <summary>
        /// Gets or sets the local file path
        /// </summary>
        /// <value>The local file path</value>
        public string LocalPath { get; set; }

        /// <summary>
        /// Gets or sets the last size of the file
        /// </summary>
        /// <value>The last size of the content.</value>
        public long LastContentSize { get; set; }

        /// <summary>
        /// Gets or sets the last file content checksum for local file
        /// </summary>
        /// <value>The last file content checksum for local file</value>
        public byte[] LastChecksum { get; set; }

        /// <summary>
        /// Gets or sets the name of the checksum algorithm.
        /// </summary>
        /// <value>The name of the checksum algorithm.</value>
        public string ChecksumAlgorithmName { get; set; }

        /// <summary>
        /// Gets or sets the last local write time in UTC
        /// </summary>
        /// <value>The last local write time in UTC</value>
        public DateTime? LastLocalWriteTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the CMIS remote object identifier
        /// </summary>
        /// <value>The CMIS remote object identifier</value>
        public string RemoteObjectId { get; set; }

        /// <summary>
        /// Gets or sets the last change token of last action make on CMIS server
        /// </summary>
        /// <value>The last change token of last action make on CMIS server</value>
        public string LastChangeToken { get; set; }

        /// <summary>
        /// Gets or sets the last remote write time in UTC
        /// </summary>
        /// <value>The last remote write time in UTC</value>
        public DateTime? LastRemoteWriteTimeUtc { get; set; }
    }
}