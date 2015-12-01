//-----------------------------------------------------------------------
// <copyright file="IMetaDataStorageConvenienceExtender.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Storage.Database {
    using System;
    using System.IO;

    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    /// <summary>
    /// Meta data storage convenience extender.
    /// </summary>
    public static class IMetaDataStorageConvenienceExtender {
        /// <summary>
        /// Gets the remote identifier by trying to read the Uuid of the given directory. If this fails or returns null,
        /// the local path is used to request the remote object id from the storage.
        /// </summary>
        /// <returns>The stored remote identifier, or null if there is no entry found.</returns>
        /// <param name="storage">Meta data storage instance.</param>
        /// <param name="info">File system item info.</param>
        public static string GetRemoteId(this IMetaDataStorage storage, IFileSystemInfo info) {
            IMappedObject mappedObject = storage.GetObject(info);
            if (mappedObject != null) {
                return mappedObject.RemoteObjectId;
            } else {
                return null;
            }
        }

        /// <summary>
        /// Gets the object based on given file Uuid or by file path.
        /// </summary>
        /// <returns>The stored object.</returns>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="info">File system info.</param>
        public static IMappedObject GetObject(this IMetaDataStorage storage, IFileSystemInfo info) {
            IMappedObject mappedObject = null;
            if (storage == null) {
                throw new ArgumentNullException("storage");
            }

            if (info == null) {
                throw new ArgumentNullException("info");
            }

            try {
                Guid? guid = info.Uuid;
                if (guid != null) {
                    mappedObject = storage.GetObjectByGuid((Guid)guid);
                }
            } catch (Exception) {
            }

            if (mappedObject == null) {
                mappedObject = storage.GetObjectByLocalPath(info);
            }

            return mappedObject;
        }
    }
}