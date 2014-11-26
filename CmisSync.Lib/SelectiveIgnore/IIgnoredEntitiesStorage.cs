namespace CmisSync.Lib.SelectiveIgnore
{
    using System;

    public interface IIgnoredEntitiesStorage
    {
        void Add(IIgnoredEntity ignore);
        void Remove(IIgnoredEntity ignore);
        IgnoredState IsIgnoredId(string objectId);
        IgnoredState IsIgnoredPath(string localPath);
    }
}