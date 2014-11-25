using CmisSync.Lib.Events;
using System.Collections.Generic;
using CmisSync.Lib.Producer.Crawler;


namespace CmisSync.Lib.Algorithms.CyclicDependencies
{
    using System;

    public interface ICycleDetector
    {
        List<List<AbstractFolderEvent>> Detect(CrawlEventCollection collection);
    }
}