using System;

using CmisSync.Lib.Storage;

using DotCMIS.Client;
using DotCMIS.Exceptions;
using System.Collections.Generic;

using log4net;

namespace CmisSync.Lib.Sync.Strategy
{
    public class RemoteSituationDetection : ISituationDetection<IObjectId>
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(RemoteSituationDetection));

        private ISession Session;
        public RemoteSituationDetection(ISession session)
        {
            if(session == null)
                throw new ArgumentNullException("The given session is null");
            Session = session;
        }

        public SituationType Analyse(IMetaDataStorage storage, IObjectId objectId) 
        {
            SituationType type = DoAnalyse(storage, objectId);
            logger.Debug(String.Format("Remote Situation is: {0}", type));
            return type;

        }

        private SituationType DoAnalyse(IMetaDataStorage storage, IObjectId objectId)
        {
            //Object has never been uploaded
            if(objectId == null) {
                return SituationType.NOCHANGE;
            }
            try {
                ICmisObject remoteObject = Session.GetObject(objectId);
                if(storage.GetFilePath(objectId.Id) == null && storage.GetFolderPath(objectId.Id) == null) {
                    return SituationType.ADDED;
                }
            }catch(CmisObjectNotFoundException) {
                return SituationType.REMOVED;
            }
            return SituationType.NOCHANGE;
        }

    }
}

