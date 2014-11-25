

namespace CmisSync.Lib.Algorithms.CyclicDependencies
{
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Producer.Crawler;

    public class CycleDetector : ICycleDetector
    {
        public CycleDetector()
        {
        }

        public List<List<AbstractFolderEvent>> Detect(CrawlEventCollection collection) {
            return null;
        }
    }
}