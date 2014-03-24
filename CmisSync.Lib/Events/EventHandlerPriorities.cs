using System;
using System.Collections.Generic;

using CmisSync.Lib.Events;
using CmisSync.Lib.Events.Filter;
using CmisSync.Lib.Sync.Strategy;

namespace CmisSync.Lib.Events {
    public static class EventHandlerPriorities {
        private static IDictionary<Type, int> map = new Dictionary<Type, int>();

        public static readonly int DEBUG = 100000;
        public static readonly int FILTER = 10000;
        public static readonly int HIGH = 2000;
        public static readonly int NORMAL = 1000;

        static EventHandlerPriorities() {
            map[typeof(DebugLoggingHandler)] = DEBUG;

            map[typeof(IgnoredFileNamesFilter)] = FILTER;
            map[typeof(IgnoredFilesFilter)] = FILTER;
            map[typeof(IgnoredFolderNameFilter)] = FILTER;
            map[typeof(InvalidFolderNameFilter)] = FILTER;
            map[typeof(IgnoredFoldersFilter)] = FILTER;

            //Higher than fallback Crawler
            map[typeof(ContentChanges)] = HIGH;
            //Accumulates events needed for Transformer
            map[typeof(ContentChangeEventAccumulator)] = HIGH;
            //Accumulates events needed for SyncStrategy
            map[typeof(FileSystemEventAccumulator)] = HIGH;

            map[typeof(ContentChangeEventTransformer)] = NORMAL;
            map[typeof(SyncScheduler)] = NORMAL;
            map[typeof(GenericHandleDoublicatedEventsFilter<,>)] = FILTER;
            map[typeof(CmisSync.Lib.Sync.Strategy.Watcher)] = NORMAL;
            map[typeof(Crawler)] = NORMAL;
            map[typeof(SyncMechanism)] = NORMAL;
        }
        
        public static int GetPriority(Type type) {
            return map[type];
        }
    }
}
