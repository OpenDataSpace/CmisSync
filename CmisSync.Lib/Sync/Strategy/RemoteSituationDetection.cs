using System;

using CmisSync.Lib.Storage;

using DotCMIS.Client;
using DotCMIS.Exceptions;
using System.Collections.Generic;

using log4net;
using CmisSync.Lib.Events;

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
            string objectId = null;
            if(actualEvent is FileEvent && (actualEvent as FileEvent).RemoteFile != null)
                objectId = (actualEvent as FileEvent).RemoteFile.Id;
            if(actualEvent is FolderEvent && (actualEvent as FolderEvent).RemoteFolder != null)
                objectId = (actualEvent as FolderEvent).RemoteFolder.Id;
            //Object has never been uploaded
            if(objectId == null) {
                return SituationType.NOCHANGE;
            }
            try {
                ICmisObject remoteObject = Session.GetObject(objectId);
                if(storage.GetObjectByRemoteId(objectId) == null )
                {
                    return SituationType.ADDED;
                }
            }catch(CmisObjectNotFoundException) {
                return SituationType.REMOVED;
            }
            return SituationType.NOCHANGE;
        }

    }
}

