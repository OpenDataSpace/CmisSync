using System;

namespace CmisSync.Lib.Events
{
    public enum MetaDataChangeType
    {
        NONE,
        CREATED,
        CHANGED,
        DELETED,
        MOVED
    }

    public enum ContentChangeType
    {
        NONE,
        CREATED,
        CHANGED,
        DELETED,
        APPENDED
    }
}

