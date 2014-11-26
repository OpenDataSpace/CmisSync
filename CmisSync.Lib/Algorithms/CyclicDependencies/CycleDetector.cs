
namespace CmisSync.Lib.Algorithms.CyclicDependencies
{
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Producer.Crawler;
    using CmisSync.Lib.Storage.Database;

    public class CycleDetector : ICycleDetector
    {
        public CycleDetector(IMetaDataStorage storage)
        {
            if (storage == null) {
                throw new ArgumentNullException("Given storage is null");
            }
        }

        public List<List<AbstractFolderEvent>> Detect(CrawlEventCollection collection) {
            var result = new List<List<AbstractFolderEvent>>();
            if (collection.mergableEvents == null) {
                return result;
            }

            foreach (var ev in collection.mergableEvents) {
                var item = ev.Value.Item1 ?? ev.Value.Item2;
                if (item.Local == MetaDataChangeType.MOVED ||
                    item.Remote == MetaDataChangeType.MOVED ||
                    item.Local == MetaDataChangeType.CHANGED ||
                    item.Remote == MetaDataChangeType.CHANGED) {
                    string process = ev.Key;
                    string resource = item.RemotePath;
                    if (item is FileEvent) {
                        var fileEvent = item as FileEvent;
                    } else if (item is FolderEvent) {
                        var folderEvent = item as FolderEvent;
                    }
                }
            }

            return result;
        }
    }
}