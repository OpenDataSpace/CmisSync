using System;
using System.IO;

using CmisSync.Lib.Storage;

using log4net;
using CmisSync.Lib.Events;

namespace CmisSync.Lib.Sync.Strategy
{
    public class LocalSituationDetection : ISituationDetection<AbstractFolderEvent>
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(LocalSituationDetection));

        private IFileSystemInfoFactory FsFactory;
        public LocalSituationDetection(IFileSystemInfoFactory fsFactory = null)
        {
            if(fsFactory == null)
                FsFactory = new FileSystemInfoFactory();
            else
                FsFactory = fsFactory;
        }

        public SituationType Analyse(IMetaDataStorage storage, AbstractFolderEvent actualEvent)
        {
            SituationType type = DoAnalyse(storage, actualEvent);
            logger.Debug(String.Format("Local Situation is: {0}", type));
            return type;
        }

        private SituationType DoAnalyse(IMetaDataStorage storage, AbstractFolderEvent actualEvent)
        {
            switch (actualEvent.Local) 
            {
                case MetaDataChangeType.CREATED:
                    return SituationType.ADDED;
                case MetaDataChangeType.DELETED:
                    return SituationType.REMOVED;
                case MetaDataChangeType.NONE:
                default:
                    return SituationType.NOCHANGE;
            }
        }
    }
}

