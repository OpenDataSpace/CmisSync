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
            /*
            actualObject.Refresh();
            if(!actualObject.Exists)
            {
                // Remove & NoChange are possible
                if(storage.GetObjectByLocalPath(actualObject) == null )
                    // Object has already been removed or wasn't ever in the storage
                    return SituationType.NOCHANGE;
                else
                    return SituationType.REMOVED;
            }
            else
            {
                // Move & Rename & Added & NoChange are possible
                if(storage.GetObjectByLocalPath(actualObject) == null )
                    return SituationType.ADDED;
            }*/
            throw new NotImplementedException();
        }
    }
}

