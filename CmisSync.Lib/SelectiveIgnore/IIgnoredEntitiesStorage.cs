
namespace CmisSync.Lib.SelectiveIgnore
{
    using System;

    public interface IIgnoredEntitiesStorage : IIgnoredEntitiesCollection
    {
        /// <summary>
        /// Adds or update an entry and deletes all children from storage.
        /// </summary>
        /// <param name="e">Ignored Entity.</param>
        void AddOrUpdateEntryAndDeleteAllChildrenFromStorage(IIgnoredEntity e);
    }
}