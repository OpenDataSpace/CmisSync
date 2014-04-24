//-----------------------------------------------------------------------
// <copyright file="RemoteObjectFetcher.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.IO;

using DotCMIS.Client;
using DotCMIS.Exceptions;

using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;
using CmisSync.Lib.Data;

using log4net;

namespace CmisSync.Lib.Sync.Strategy { 
    public class RemoteObjectFetcher : SyncEventHandler {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RemoteObjectFetcher));

        private IMetaDataStorage storage;
        private ISession session;

        private ICmisObject GetRemoteObject(ISyncEvent e) {
            if(e is FileEvent) {
                return (e as FileEvent).RemoteFile;
            }
            return (e as FolderEvent).RemoteFolder;
        }

        private void SetRemoteObject(ISyncEvent e, ICmisObject remote) {
            if(e is FileEvent) {
                (e as FileEvent).RemoteFile = remote as IDocument;
            }else{
                (e as FolderEvent).RemoteFolder = remote as IFolder;
            }
        }

        private string FetchIdFromStorage(ISyncEvent e) {
            IFileSystemInfo path = null;
            if(e is FileEvent) {
                path = (e as FileEvent).LocalFile;
            }
            else if( e is FolderEvent)
            {
                path = (e as FolderEvent).LocalFolder;
            }
            if(path != null)
            {
                IMappedObject savedObject = this.storage.GetObjectByLocalPath(path);
                if(savedObject != null) {
                    return savedObject.RemoteObjectId;
                }
            }
            return null;
        }

        public override bool Handle(ISyncEvent e) {
            if(!(e is FileEvent || e is FolderEvent)){
                return false;
            }
            ICmisObject remote = GetRemoteObject(e);
            //already there, no need to GetObject
            if (remote != null) {
                return false;
            }

            string id = FetchIdFromStorage(e);
            if(id != null) {
                try {
                    remote = session.GetObject(id);
                } catch (CmisObjectNotFoundException) {
                    //Deleted on Server, this is ok
                    return false;
                }
                SetRemoteObject(e, remote);
            }
            return false;
        }

        public RemoteObjectFetcher(ISession session, IMetaDataStorage storage) {
            if(session == null)
                throw new ArgumentNullException("Session instance is needed , but was null");
            if(storage == null)
                throw new ArgumentNullException("MetaDataStorage instance is needed, but was null");
            this.session = session;
            this.storage = storage;
        } 
    }
}
