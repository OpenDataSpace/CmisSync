

namespace CmisSync.Lib.SelectiveIgnore
{
    using System;

    using CmisSync.Lib.Storage.Database;

    using DotCMIS.Client;

    /// <summary>
    /// Ignored entities storage.
    /// </summary>
    public class IgnoredEntitiesStorage : IIgnoredEntitiesStorage
    {
        private IIgnoredEntitiesCollection collection;
        private IMetaDataStorage storage;

        public IgnoredEntitiesStorage(IIgnoredEntitiesCollection collection, IMetaDataStorage storage) {
            if (collection == null) {
                throw new ArgumentNullException("Given collection is null");
            }

            if (storage == null) {
                throw new ArgumentNullException("Given storage is null");
            }

            this.collection = collection;
            this.storage = storage;
        }

        /// <summary>
        /// Adds or update an entry and deletes all children from storage.
        /// </summary>
        /// <param name="e">Ignored Entity.</param>
        public void AddOrUpdateEntryAndDeleteAllChildrenFromStorage(IIgnoredEntity e) {
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
        public void Add(IIgnoredEntity ignore) {
            this.collection.Add(ignore);
        }

        /// <summary>
        /// Remove the specified ignore form the given collection
        /// </summary>
        /// <param name="ignore">Ignored entity.</param>
        public void Remove(IIgnoredEntity ignore) {
            this.collection.Remove(ignore);
        }

        /// <summary>
        /// Remove the specified objectId from the given collection
        /// </summary>
        /// <param name="objectId">Object identifier.</param>
        public void Remove(string objectId) {
            this.collection.Remove(objectId);
        }

        public IgnoredState IsIgnored(IDocument doc) {
            return this.collection.IsIgnored(doc);
        }

        public IgnoredState IsIgnored(IFolder folder) {
            return this.collection.IsIgnored(folder);
        }

        public IgnoredState IsIgnoredId(string objectId) {
            return this.collection.IsIgnoredId(objectId);
        }

        public IgnoredState IsIgnoredPath(string localPath) {
            return this.collection.IsIgnoredPath(localPath);
        }
    }
}