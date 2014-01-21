using System;

using CmisSync.Lib.Storage;

using DotCMIS.Client;

namespace CmisSync.Lib.Sync.Strategy
{
    public class RemoteSituationDetection : ISituationDetection<string>
    {
        private ISession Session;
        public RemoteSituationDetection(ISession session)
        {
            if(session == null)
                throw new ArgumentNullException("The given session is null");
            Session = session;
        }

        public SituationType Analyse(MetaDataStorage storage, string objectId)
        {
            throw new NotImplementedException();
        }
    }
}

