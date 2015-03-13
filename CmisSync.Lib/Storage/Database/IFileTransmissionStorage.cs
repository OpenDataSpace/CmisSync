//-----------------------------------------------------------------------
// <copyright file="IFileTransmissionStorage.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Storage.Database
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using CmisSync.Lib.Storage.Database.Entities;

    /// <summary>
    /// Interface for the saved <c>IFileTransmissionObject</c> list in the storage
    /// </summary>
    /// <remarks>
    /// The <c>IFileTransmissionObject.RemoteObjectId</c> is the primary key for the saved <code>IFileTransmissionObject</code> list.
    /// </remarks>
    public interface IFileTransmissionStorage
    {
        /// <summary>
        /// Get the saved <c>IFileTransmissionObject</c> list
        /// </summary>
        /// <returns>The saved <c>IFileTransmissionObject</c> list</returns>
        IList<IFileTransmissionObject> GetObjectList();

        /// <summary>
        /// Save the <c>IFileTransmissionObject</c> object
        /// </summary>
        /// <remarks>
        /// If one saved <c>IFileTransmissionObject</c> has the same <c>IFileTransmissionObject.RemoteObjectId</c> as <paramref name="obj"/>, it will be replaced with <paramref name="obj"/>
        /// </remarks>
        /// <param name="obj">
        /// The <c>IFileTransmissionObject</c> object to be saved
        /// </param>
        void SaveObject(IFileTransmissionObject obj);

        /// <summary>
        /// Get the saved <c>IFileTransmissionObject</c> that has the <c>IFileTransmissionObject.RemoteObjectId</c> with <paramref name="remoteObjectId"/>
        /// </summary>
        /// <param name="remoteObjectId">
        /// <c>IFileTransmissionObject.RemoteObjectId</c> value
        /// </param>
        IFileTransmissionObject GetObjectByRemoteObjectId(string remoteObjectId);

        /// <summary>
        /// Get the saved <c>IFileTransmissionObject</c> that has the <c>IFileTransmissionObject.LocalPath</c> with <paramref name="localPath"/>
        /// </summary>
        /// <param name="localPath">
        /// <c>IFileTransmissionObject.LocalPath</c> value
        /// </param>
        IFileTransmissionObject GetObjectByLocalPath(string localPath);

        /// <summary>
        /// Remove any saved <c>IFileTransmissionObject</c> that has the <c>IFileTransmissionObject.RemoteObjectId</c> with <paramref name="remoteObjectId"/>
        /// </summary>
        /// <param name="remoteObjectId">
        /// <c>IFileTransmissionObject.RemoteObjectId</c> value
        /// </param>
        void RemoveObjectByRemoteObjectId(string remoteObjectId);

        /// <summary>
        /// Remove all saved <c>IFileTransmissionObject</c> list
        /// </summary>
        void ClearObjectList();

        /// <summary>
        /// Chunk size for file transmission
        /// </summary>
        long ChunkSize { get; }
    }
}
