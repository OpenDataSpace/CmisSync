using System;
using System.IO;
using System.Collections.Generic;

using DotCMIS;
using DotCMIS.Client;
using DotCMIS.Client.Impl;

using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;
using CmisSync.Lib.Data;


namespace CmisSync.Lib.Sync.Solver
{
    public class LocalObjectAdded : ISolver
    {
        public virtual void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFile, IObjectId remoteId){
            // Create new remote object
            if((localFile.Attributes & FileAttributes.Directory) == FileAttributes.Directory) {
                IDirectoryInfo localDirInfo = localFile as IDirectoryInfo;
                IDirectoryInfo parent = localDirInfo.Parent;
                IMappedFolder mappedParent = storage.GetObjectByLocalPath(parent) as IMappedFolder;
                // Create remote folder
                Dictionary<string, object> properties = new Dictionary<string, object>();
                properties.Add(PropertyIds.Name, localDirInfo.Name);
                properties.Add(PropertyIds.ObjectTypeId, "cmis:folder");
                properties.Add(PropertyIds.CreationDate, "");
                properties.Add(PropertyIds.LastModificationDate, "");
                session.CreateFolder(properties, new ObjectId(mappedParent.RemoteObjectId));
                IMappedFolder mappedFolder = new MappedFolder(mappedParent, localDirInfo.Name) { Name = localDirInfo.Name};
                mappedParent.Children.Add(mappedFolder);
            }
            else if((localFile.Attributes & FileAttributes.Normal) == FileAttributes.Normal) {
                // Create empty remote file
                string remotePath = storage.Matcher.CreateRemotePath(localFile.FullName);
                //session.CreateDocument();
            }
        }
    }
}

