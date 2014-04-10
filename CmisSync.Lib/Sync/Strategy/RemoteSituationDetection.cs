using System;

using CmisSync.Lib.Storage;

using DotCMIS.Client;
using DotCMIS.Exceptions;
using System.Collections.Generic;

using log4net;
using CmisSync.Lib.Events;
using CmisSync.Lib.Data;

namespace CmisSync.Lib.Sync.Strategy
{
    public class RemoteSituationDetection : ISituationDetection<AbstractFolderEvent>
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(RemoteSituationDetection));

        private ISession Session;
        public RemoteSituationDetection(ISession session)
        {
            if(session == null)
                throw new ArgumentNullException("The given session is null");
            Session = session;
        }

        public SituationType Analyse(IMetaDataStorage storage, AbstractFolderEvent actualEvent)
        {
            SituationType type = DoAnalyse(storage, actualEvent);
            logger.Debug(String.Format("Remote Situation is: {0}", type));
            return type;

        }

        private SituationType DoAnalyse(IMetaDataStorage storage, AbstractFolderEvent actualEvent)
        {
            switch (actualEvent.Remote) 
            {
            case MetaDataChangeType.CREATED:
                if(actualEvent is FileEvent)
                    return (IsSavedFileEqual(storage, (actualEvent as FileEvent).RemoteFile)) ? SituationType.NOCHANGE : SituationType.ADDED;
            case MetaDataChangeType.DELETED:
                return SituationType.REMOVED;
            case MetaDataChangeType.NONE:
            default:
                return SituationType.NOCHANGE;
            }
        }

        private bool IsSavedFileEqual(IMetaDataStorage storage, IDocument doc)
        {
            var mappedFile = storage.GetObjectByRemoteId(doc.Id) as IMappedFile;
            if( mappedFile != null &&
               mappedFile.LastRemoteWriteTimeUtc == doc.LastModificationDate &&
               mappedFile.Name == doc.Name &&
               mappedFile.LastChangeToken == doc.ChangeToken)
            {
                return true;
            } else {
                return false;
            }
        }

    }
}

