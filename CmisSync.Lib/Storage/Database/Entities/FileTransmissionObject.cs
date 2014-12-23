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

namespace CmisSync.Lib.Storage.Database.Entities
{
    using System;
    using System.Linq;
    using System.IO;

    using DotCMIS.Client;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage.FileSystem;

    /// <summary>
    /// Implementation of <c>IFileTransmissionObject</c>
    /// </summary>
    [Serializable]
    public class FileTransmissionObject : IFileTransmissionObject
    {
        public FileTransmissionObject(FileTransmissionType type, IFileInfo localFile, IDocument remoteFile)
        {
            if (localFile == null)
            {
                throw new ArgumentNullException("localFile");
            }
            if (!localFile.Exists)
            {
                throw new ArgumentException(string.Format("'{0} file does not exist", localFile.FullName), "localFile");
            }

            if (remoteFile == null)
            {
                throw new ArgumentNullException("remoteFile");
            }
            if (remoteFile.Id == null)
            {
                throw new ArgumentNullException("remoteFile.Id");
            }
            if (string.IsNullOrEmpty(remoteFile.Id))
            {
                throw new ArgumentException("empty string", "remoteFile.Id");
            }

            Type = type;
            LocalPath = localFile.FullName;
            LastContentSize = localFile.Length;
            LastLocalWriteTimeUtc = localFile.LastWriteTimeUtc;
            RemoteObjectId = remoteFile.Id;
            LastChangeToken = remoteFile.ChangeToken;
            LastRemoteWriteTimeUtc = remoteFile.LastModificationDate;
            if (LastRemoteWriteTimeUtc != null)
            {
                LastRemoteWriteTimeUtc = LastRemoteWriteTimeUtc.GetValueOrDefault().ToUniversalTime();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MappedObject"/> class.
        /// </summary>
        [Obsolete("Must not be used manually. This constructor should be used for serialization only.", true)]
        public FileTransmissionObject()
        {
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (this == obj)
            {
                return true;
            }

            // If parameter cannot be casted to FileTransmissionObject return false.
            FileTransmissionObject o = obj as FileTransmissionObject;
            if (o == null)
            {
                return false;
            }

            return Type.Equals(o.Type) && 
                object.Equals(LocalPath, o.LocalPath) &&
                ((LastChecksum == null && o.LastChecksum == null) || (LastChecksum != null && o.LastChecksum != null && LastChecksum.SequenceEqual(o.LastChecksum))) &&
                object.Equals(ChecksumAlgorithmName, o.ChecksumAlgorithmName) &&
                object.Equals(LastLocalWriteTimeUtc, o.LastLocalWriteTimeUtc) &&
                object.Equals(RemoteObjectId, o.RemoteObjectId) &&
                object.Equals(LastChangeToken, o.LastChangeToken) &&
                object.Equals(LastRemoteWriteTimeUtc, o.LastRemoteWriteTimeUtc);
        }

        public FileTransmissionType Type { get; set; }

        public string LocalPath { get; set; }

        public long LastContentSize { get; set; }

        public byte[] LastChecksum { get; set; }

        public string ChecksumAlgorithmName { get; set; }

        public DateTime? LastLocalWriteTimeUtc { get; set; }

        public string RemoteObjectId { get; set; }

        public string LastChangeToken { get; set; }

        public DateTime? LastRemoteWriteTimeUtc { get; set; }
    }
}
