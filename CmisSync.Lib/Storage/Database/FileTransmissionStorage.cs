//-----------------------------------------------------------------------
// <copyright file="FileTransmissionStorage.cs" company="GRAU DATA AG">
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
    using System.Collections.Generic;

    using CmisSync.Lib.Storage.Database.Entities;

    using DBreeze;
    using DBreeze.DataTypes;

    /// <summary>
    /// File transmission storage.
    /// </summary>
    public class FileTransmissionStorage : IFileTransmissionStorage {
        private const string FileTransmissionObjectsTable = "FileTransmissionObjects";

        private DBreezeEngine engine;

        static FileTransmissionStorage() {
            DBreezeInitializerSingleton.Init();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Storage.Database.FileTransmissionStorage"/> class.
        /// </summary>
        /// <param name="engine">DBreeze engine. Must not be null.</param>
        /// <param name="chunkSize">Upload chunk size.</param>
        [CLSCompliant(false)]
        public FileTransmissionStorage(DBreezeEngine engine, long chunkSize = CmisSync.Lib.Config.Config.DefaultChunkSize) {
            if (engine == null) {
                throw new ArgumentNullException("engine");
            }

            if (chunkSize < 1) {
                throw new ArgumentException(string.Format("Given chunkSize \"{0}\" is too low", chunkSize),"chunkSize");
            }

            this.engine = engine;
            this.ChunkSize = chunkSize;
        }

        /// <summary>
        /// Gets or sets the chunk size for file transmission
        /// </summary>
        /// <value>The size of the chunk.</value>
        public long ChunkSize { get; private set; }

        /// <summary>
        /// Gets the file transmission object list.
        /// </summary>
        /// <returns>The object list.</returns>
        public IList<IFileTransmissionObject> GetObjectList() {
            List<IFileTransmissionObject> objects = new List<IFileTransmissionObject>();

            using (var tran = this.engine.GetTransaction()) {
                foreach (var row in tran.SelectForward<string, DbCustomSerializer<FileTransmissionObject>>(FileTransmissionObjectsTable)) {
                    var value = row.Value;
                    if (value == null) {
                        continue;
                    }

                    var data = value.Get;
                    if (data == null) {
                        continue;
                    }

                    objects.Add(data);
                }
            }

            return objects;
        }

        /// <summary>
        /// Saves the object.
        /// </summary>
        /// <param name="obj">File transmission object.</param>
        public void SaveObject(IFileTransmissionObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            if (obj.LocalPath == null) {
                throw new ArgumentNullException("obj.LocalPath");
            }

            if (string.IsNullOrEmpty(obj.LocalPath)) {
                throw new ArgumentException("empty string", "obj.LocalPath");
            }

            if (obj.RemoteObjectId == null) {
                throw new ArgumentNullException("obj.RemoteObjectId");
            }

            if (string.IsNullOrEmpty(obj.RemoteObjectId)) {
                throw new ArgumentException("empty string", "obj.RemoteObjectId");
            }

            if (!(obj is FileTransmissionObject)) {
                throw new ArgumentException("require FileTransmissionObject type", "obj");
            }

            using (var tran = this.engine.GetTransaction()) {
                tran.Insert<string, DbCustomSerializer<FileTransmissionObject>>(FileTransmissionObjectsTable, obj.RemoteObjectId, obj as FileTransmissionObject);
                tran.Commit();
            }
        }

        /// <summary>
        /// Gets the file transmission object by given remote object identifier.
        /// </summary>
        /// <returns>The file transmission object by given remote object identifier.</returns>
        /// <param name="remoteObjectId">Remote object identifier.</param>
        public IFileTransmissionObject GetObjectByRemoteObjectId(string remoteObjectId) {
            using (var tran = this.engine.GetTransaction()) {
                DbCustomSerializer<FileTransmissionObject> value = tran.Select<string, DbCustomSerializer<FileTransmissionObject>>(FileTransmissionObjectsTable, remoteObjectId).Value;
                if (value == null) {
                    return null;
                }

                return value.Get;
            }
        }

        /// <summary>
        /// Gets the file transmission object by given local path.
        /// </summary>
        /// <returns>The file transmission object of given local path.</returns>
        /// <param name="localPath">Local path.</param>
        public IFileTransmissionObject GetObjectByLocalPath(string localPath) {
            using (var tran = this.engine.GetTransaction()) {
                foreach (var row in tran.SelectForward<string, DbCustomSerializer<FileTransmissionObject>>(FileTransmissionObjectsTable)) {
                    var value = row.Value;
                    if (value == null) {
                        continue;
                    }

                    var data = value.Get;
                    if (data == null) {
                        continue;
                    }

                    if (data.LocalPath == localPath) {
                        return data;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Removes the file transmission object by remote object identifier.
        /// </summary>
        /// <param name="remoteObjectId">Remote object identifier.</param>
        public void RemoveObjectByRemoteObjectId(string remoteObjectId) {
            if (remoteObjectId == null) {
                throw new ArgumentNullException("remoteObjectId");
            }

            if (string.IsNullOrEmpty(remoteObjectId)) {
                throw new ArgumentException("empty string", "remoteObjectId");
            }

            using (var tran = this.engine.GetTransaction()) {
                tran.RemoveKey(FileTransmissionObjectsTable, remoteObjectId);
                tran.Commit();
            }
        }

        /// <summary>
        /// Clears the object list.
        /// </summary>
        public void ClearObjectList() {
            using (var tran = this.engine.GetTransaction()) {
                tran.RemoveAllKeys(FileTransmissionObjectsTable, true);
                tran.Commit();
            }
        }
    }
}