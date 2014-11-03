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

    using CmisSync.Lib.PathMatcher;
    using CmisSync.Lib.Events;

    /// <summary>
    /// Implementation of <c>IFileTransmissionObject</c>
    /// </summary>
    [Serializable]
    public class FileTransmissionObject : IFileTransmissionObject
    {
        public FileTransmissionObject(FileTransmissionType type, string localPath, IDocument remoteFile, IPathMatcher matcher)
        {
            if (localPath == null)
            {
                throw new ArgumentNullException("localPath");
            }
            if (string.IsNullOrEmpty(localPath))
            {
                throw new ArgumentException("empty string", "localPath");
            }

            if (remoteFile == null)
            {
                throw new ArgumentNullException("remoteFile");
            }
            if (remoteFile.Paths == null)
            {
                throw new ArgumentNullException("remoteFile.Paths");
            }
            if (remoteFile.Paths.Count == 0)
            {
                throw new ArgumentException("zero size", "remoteFile.Paths");
            }
            if (remoteFile.Paths[0] == null)
            {
                throw new ArgumentNullException("remoteFile.Paths[0]");
            }
            if (string.IsNullOrEmpty(remoteFile.Paths[0]))
            {
                throw new ArgumentException("empty string", "remoteFile.Paths[0]");
            }

            if (matcher == null)
            {
                throw new ArgumentNullException("Path matcher is null");
            }

            if (!matcher.CanCreateLocalPath(remoteFile))
            {
                throw new ArgumentException(string.Format("'{0}' does not match", remoteFile.Paths[0]), "remoteFile");
            }

            if (!matcher.CanCreateRemotePath(localPath))
            {
                throw new ArgumentException(string.Format("'{0}' does not match", localPath), "localPath");
            }

            if (!matcher.Matches(localPath, remoteFile.Paths[0]))
            {
                throw new ArgumentException(string.Format("Local path {0} does not match the remote file {1}", localPath, remoteFile.Paths[0]));
            }

            if (!File.Exists(localPath))
            {
                throw new ArgumentException(string.Format("'{0} file does not exist", localPath), "localPath");
            }

            RelativePath = matcher.GetRelativeLocalPath(localPath);
            Type = type;
            LastLocalWriteTimeUtc = File.GetLastWriteTimeUtc(localPath);
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
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to MappedObjectData return false.
            FileTransmissionObject o = obj as FileTransmissionObject;
            if (o == null)
            {
                return false;
            }

            return object.Equals(RelativePath, o.RelativePath) &&
                Type.Equals(o.Type) &&
                ((LastChecksum == null && o.LastChecksum == null) || (LastChecksum != null && o.LastChecksum != null && LastChecksum.SequenceEqual(o.LastChecksum))) &&
                object.Equals(ChecksumAlgorithmName, o.ChecksumAlgorithmName) &&
                object.Equals(LastLocalWriteTimeUtc, o.LastLocalWriteTimeUtc) &&
                object.Equals(RemoteObjectId, o.RemoteObjectId) &&
                object.Equals(LastChangeToken, o.LastChangeToken) &&
                object.Equals(LastRemoteWriteTimeUtc, o.LastRemoteWriteTimeUtc);
        }

        public string RelativePath { get; set; }

        public FileTransmissionType Type { get; set; }

        public byte[] LastChecksum { get; set; }

        public string ChecksumAlgorithmName { get; set; }

        public DateTime? LastLocalWriteTimeUtc { get; set; }

        public string RemoteObjectId { get; set; }

        public string LastChangeToken { get; set; }

        public DateTime? LastRemoteWriteTimeUtc { get; set; }
    }
}
