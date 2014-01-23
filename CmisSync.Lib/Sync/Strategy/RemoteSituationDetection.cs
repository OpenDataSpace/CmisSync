using System;

using CmisSync.Lib.Storage;

using DotCMIS.Client;
using DotCMIS.Exceptions;

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

        public SituationType Analyse(IMetaDataStorage storage, string objectId)
        {
            try {
                ICmisObject remoteObject = Session.GetObject(objectId);
                if(storage.GetFilePath(objectId) == null && storage.GetFolderPath(objectId) == null)
                    return SituationType.ADDED;
                var document = remoteObject as IDocument;
                if(document != null)
                {
                    var savedPath = storage.GetFilePath(objectId);
                    if(savedPath == null)
                        return SituationType.ADDED;
                    throw new NotImplementedException();
                }
                var folder = remoteObject as IFolder;
                if(folder != null)
                {
                    var savedPath = storage.GetFolderPath(objectId);
                    if(savedPath == null)
                        return SituationType.ADDED;
                    throw new NotImplementedException();
                }
            }catch(CmisObjectNotFoundException) {
                if(storage.GetFilePath(objectId) == null && storage.GetFolderPath(objectId) == null)
                {
                    return SituationType.NOCHANGE;
                }
                else
                {
                    return SituationType.REMOVED;
                }
            }
            return SituationType.NOCHANGE;
        }
    }
}

