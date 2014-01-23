using System;
using CmisSync.Lib.Storage;

namespace CmisSync.Lib.Sync.Strategy
{
    public interface ISituationDetection<T>
    {
        SituationType Analyse(IMetaDataStorage storage, T actualObject);
    }

    public enum SituationType {
        NOCHANGE,
        ADDED,
        CHANGED,
        RENAMED,
        MOVED,
        REMOVED
    }
}

