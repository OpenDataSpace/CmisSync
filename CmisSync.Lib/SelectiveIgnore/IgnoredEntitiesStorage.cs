//-----------------------------------------------------------------------
// <copyright file="IgnoredEntitiesStorage.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.SelectiveIgnore {
    using System;

    using CmisSync.Lib.Storage.Database;

    using DotCMIS.Client;

    /// <summary>
    /// Ignored entities storage.
    /// </summary>
    public class IgnoredEntitiesStorage : IIgnoredEntitiesStorage {
        private IIgnoredEntitiesCollection collection;
        private IMetaDataStorage storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.SelectiveIgnore.IgnoredEntitiesStorage"/> class.
        /// </summary>
        /// <param name="collection">Ignored entries collection.</param>
        /// <param name="storage">Meta data storage.</param>
        public IgnoredEntitiesStorage(IIgnoredEntitiesCollection collection, IMetaDataStorage storage) {
            if (collection == null) {
                throw new ArgumentNullException("collection");
            }

            if (storage == null) {
                throw new ArgumentNullException("storage");
            }

            this.collection = collection;
            this.storage = storage;
        }

        /// <summary>
        /// Adds or update an entry and deletes all children from storage.
        /// </summary>
        /// <param name="e">Ignored Entity.</param>
        public void AddOrUpdateEntryAndDeleteAllChildrenFromStorage(AbstractIgnoredEntity e) {
            this.Add(e);
            var mappedEntry = this.storage.GetObjectByRemoteId(e.ObjectId);
            if (mappedEntry != null) {
                var children = this.storage.GetChildren(mappedEntry);
                if (children != null) {
                    foreach (var child in children) {
                        this.storage.RemoveObject(child);
                    }
                }
            }
        }

        /// <summary>
        /// Add the specified ignore to the given collection
        /// </summary>
        /// <param name="ignore">Ignored entity.</param>
        public void Add(AbstractIgnoredEntity ignore) {
            this.collection.Add(ignore);
        }

        /// <summary>
        /// Remove the specified ignore form the given collection
        /// </summary>
        /// <param name="ignore">Ignored entity.</param>
        public void Remove(AbstractIgnoredEntity ignore) {
            this.collection.Remove(ignore);
        }

        /// <summary>
        /// Remove the specified objectId from the given collection
        /// </summary>
        /// <param name="objectId">Object identifier.</param>
        public void Remove(string objectId) {
            this.collection.Remove(objectId);
        }

        /// <summary>
        /// Determines whether the given Document is ignored.
        /// </summary>
        /// <returns><c>true</c> if the given doc is ignored; otherwise <c>false</c></returns>
        /// <param name="doc">Document to be checked.</param>
        public IgnoredState IsIgnored(IDocument doc) {
            return this.collection.IsIgnored(doc);
        }

        /// <summary>
        /// Determines whether the given folder is ignored.
        /// </summary>
        /// <returns><c>true</c> if the given folder is ignored; otherwise, <c>false</c>.</returns>
        /// <param name="folder">Folder to be checked.</param>
        public IgnoredState IsIgnored(IFolder folder) {
            return this.collection.IsIgnored(folder);
        }

        /// <summary>
        /// Determines whether the object with the given objectId is ignored.
        /// </summary>
        /// <returns><c>true</c> if the object with the given objectId is ignored; otherwise, <c>false</c>.</returns>
        /// <param name="objectId">Object identifier.</param>
        public IgnoredState IsIgnoredId(string objectId) {
            return this.collection.IsIgnoredId(objectId);
        }

        /// <summary>
        /// Determines whether this the ignored path is ignored.
        /// </summary>
        /// <returns><c>true</c> if the local path is ignored; otherwise, <c>false</c>.</returns>
        /// <param name="localPath">Local path.</param>
        public IgnoredState IsIgnoredPath(string localPath) {
            return this.collection.IsIgnoredPath(localPath);
        }
    }
}