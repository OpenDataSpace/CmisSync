
namespace CmisSync.Lib.SelectiveIgnore
{
    using System;

    public interface IIgnoredEntity
    {
        string ObjectId { get; }
        string LocalPath { get; }
    }
}