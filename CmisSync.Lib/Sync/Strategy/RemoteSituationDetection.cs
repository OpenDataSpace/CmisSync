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
            /*
            //Object has never been uploaded
            if(objectId == null) {
                return SituationType.NOCHANGE;
            }
            try {
                ICmisObject remoteObject = Session.GetObject(objectId);
                if(storage.GetObjectByRemoteId(objectId.Id) == null )
                {
                    return SituationType.ADDED;
                }
            }catch(CmisObjectNotFoundException) {
                return SituationType.REMOVED;
            }
            return SituationType.NOCHANGE;
            */
            throw new NotImplementedException();
        }

    }
}

