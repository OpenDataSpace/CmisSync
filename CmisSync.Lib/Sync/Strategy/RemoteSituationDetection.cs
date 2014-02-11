using System;

using CmisSync.Lib.Storage;

using DotCMIS.Client;
using DotCMIS.Exceptions;
using System.Collections.Generic;

namespace CmisSync.Lib.Sync.Strategy
{
    public class RemoteSituationDetection : ISituationDetection<IObjectId>
    {
        private ISession Session;
        public RemoteSituationDetection(ISession session)
        {
            if(session == null)
                throw new ArgumentNullException("The given session is null");
            Session = session;
        }

        public SituationType Analyse(IMetaDataStorage storage, IObjectId objectId)
        {
            try {
                ICmisObject remoteObject = Session.GetObject(objectId);
                if(storage.GetFilePath(objectId.Id) == null && storage.GetFolderPath(objectId.Id) == null)
                    return SituationType.ADDED;
                var document = remoteObject as IDocument;
                if(document != null)
                {
                    var savedPath = storage.GetFilePath(objectId.Id);
                    if(savedPath == null)
                        return SituationType.ADDED;
                    if(DocumentRenamed(savedPath, document.Paths))
                        return SituationType.RENAMED;
                    if(DocumentMoved(savedPath, document.Paths))
                        return SituationType.MOVED;
                    return SituationType.NOCHANGE;
                }
                var folder = remoteObject as IFolder;
                if(folder != null)
                {
                    var savedPath = storage.GetFolderPath(objectId.Id);
                    if(savedPath == null)
                        return SituationType.ADDED;
                    if(FolderRenamed(savedPath, folder.Path))
                        return SituationType.RENAMED;
                    if(FolderMoved(savedPath, folder.Path))
                        return SituationType.MOVED;
                    return SituationType.NOCHANGE;
                }
            }catch(CmisObjectNotFoundException) {
                if(storage.GetFilePath(objectId.Id) == null && storage.GetFolderPath(objectId.Id) == null)
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

        private bool DocumentRenamed(string savedPath, IList<string> actualPaths) {
            throw new NotImplementedException();
        }

        private bool DocumentMoved(string savedPath, IList<string> actualPaths) {
            throw new NotImplementedException();
        }

        private bool FolderRenamed(string savedPath, string actualPath) {
            throw new NotImplementedException();
        }

        private bool FolderMoved(string savedPath, string actualPath) {
            throw new NotImplementedException();
        }
    }
}

