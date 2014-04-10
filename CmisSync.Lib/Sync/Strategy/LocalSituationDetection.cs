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
            switch(actualEvent.Local)
            {
            case MetaDataChangeType.CREATED:
                return SituationType.ADDED;
            case MetaDataChangeType.CHANGED:
                return SituationType.CHANGED;
            case MetaDataChangeType.NONE:
                return SituationType.NOCHANGE;
            }
            IFileSystemInfo actualObject = null;
            if(actualEvent is FileEvent)
                actualObject = (actualEvent as FileEvent).LocalFile;
            if(actualEvent is FolderEvent)
                actualObject = (actualEvent as FolderEvent).LocalFolder;
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
            }
            throw new NotImplementedException();
        }
    }
}

