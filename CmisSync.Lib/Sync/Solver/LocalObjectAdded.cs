//-----------------------------------------------------------------------
// <copyright file="LocalObjectAdded.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Sync.Solver
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Client.Impl;

    /// <summary>
    /// Solver to handle the situation of a locally added file/folderobject.
    /// </summary>
    public class LocalObjectAdded : ISolver
    {
        public virtual void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFile, IObjectId remoteId)
        {
            // Create new remote object
            if((localFile.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                IDirectoryInfo localDirInfo = localFile as IDirectoryInfo;
                IDirectoryInfo parent = localDirInfo.Parent;
                IMappedFolder mappedParent = storage.GetObjectByLocalPath(parent) as IMappedFolder;

                // Create remote folder
                Dictionary<string, object> properties = new Dictionary<string, object>();
                properties.Add(PropertyIds.Name, localDirInfo.Name);
                properties.Add(PropertyIds.ObjectTypeId, "cmis:folder");
                properties.Add(PropertyIds.CreationDate, string.Empty);
                properties.Add(PropertyIds.LastModificationDate, string.Empty);
                session.CreateFolder(properties, new ObjectId(mappedParent.RemoteObjectId));
                IMappedFolder mappedFolder = new MappedFolder(null, storage) 
                {
                    Name = localDirInfo.Name,
                    ParentId = mappedParent.RemoteObjectId
                };
                storage.SaveMappedObject(mappedFolder);
            }
            else if((localFile.Attributes & FileAttributes.Normal) == FileAttributes.Normal)
            {
                // Create empty remote file
                string remotePath = storage.Matcher.CreateRemotePath(localFile.FullName);
                //session.CreateDocument();
            }
        }
    }
}
