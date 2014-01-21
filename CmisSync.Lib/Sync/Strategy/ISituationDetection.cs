using System;
using CmisSync.Lib.Storage;

namespace CmisSync.Lib.Sync.Strategy
{
    public interface ISituationDetection<T>
    {
        SituationType Analyse(MetaDataStorage storage, T actualObject);
    }

    public enum SituationType {
        NOCHANGE,
        CREATED,
        CHANGED,
        RENAMED,
        MOVED,
        REMOVED
    }
}

