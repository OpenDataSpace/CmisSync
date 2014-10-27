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

namespace CmisSync.Lib.Storage.Database.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

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
        }

        public string RelativePath { get; private set; }

        public FileTransmissionType Type { get; private set; }

        public byte[] LastChecksum { get; set; }

        public string ChecksumAlgorithmName { get; set; }

        public DateTime? LastLocalWriteTimeUtc { get; set; }

        public string RemoteObjectId { get; private set; }

        public string LastChangeToken { get; set; }

        public DateTime? LastRemoteWriteTimeUtc { get; set; }
    }
}
