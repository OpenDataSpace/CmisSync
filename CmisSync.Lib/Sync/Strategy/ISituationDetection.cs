using System;
using CmisSync.Lib.Storage;
using CmisSync.Lib.Events;

namespace CmisSync.Lib.Sync.Strategy
{
    public interface ISituationDetection<T> where T : AbstractFolderEvent
    {
        SituationType Analyse(IMetaDataStorage storage, T actualEvent);
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

