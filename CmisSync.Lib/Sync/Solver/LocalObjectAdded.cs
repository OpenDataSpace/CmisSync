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
    using DotCMIS.Enums;

    /// <summary>
    /// Solver to handle the situation of a locally added file/folderobject.
    /// </summary>
    public class LocalObjectAdded : ISolver
    {
        /// <summary>
        /// Solve the situation of a local object added and should be uploaded by using the session, storage, localFile and remoteId.
        /// </summary>
        /// <param name="session">Cmis session instance.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="localFile">Local file.</param>
        /// <param name="remoteId">Remote identifier.</param>
        public virtual void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFile, IObjectId remoteId)
        {
            // Create new remote object
            if(localFile is IDirectoryInfo)
            {
                IDirectoryInfo localDirInfo = localFile as IDirectoryInfo;
                IDirectoryInfo parent = localDirInfo.Parent;
                IMappedObject mappedParent = storage.GetObjectByLocalPath(parent);

                // Create remote folder
                Dictionary<string, object> properties = new Dictionary<string, object>();
                properties.Add(PropertyIds.Name, localDirInfo.Name);
                properties.Add(PropertyIds.ObjectTypeId, "cmis:folder");
                IFolder folder = session.GetObject(session.CreateFolder(properties, new ObjectId(mappedParent.RemoteObjectId))) as IFolder;
                Guid uuid = Guid.Empty;
                if (localDirInfo.IsExtendedAttributeAvailable()) {
                    uuid = Guid.NewGuid();
                    localDirInfo.SetExtendedAttribute(MappedObject.ExtendedAttributeKey, uuid.ToString());
                }

                localDirInfo.LastWriteTimeUtc = folder.LastModificationDate != null ? (DateTime)folder.LastModificationDate : localDirInfo.LastWriteTimeUtc;

                MappedObject mappedFolder = new MappedObject(
                    localDirInfo.Name,
                    folder.Id,
                    MappedObjectType.Folder,
                    mappedParent.RemoteObjectId,
                    folder.ChangeToken)
                {
                    Guid = uuid,
                    LastRemoteWriteTimeUtc = folder.LastModificationDate,
                    LastLocalWriteTimeUtc = localDirInfo.LastWriteTimeUtc
                };
                storage.SaveMappedObject(mappedFolder);
            }
            else if(localFile is IFileInfo)
            {
                // Create empty remote file
                IFileInfo localFileInfo = localFile as IFileInfo;
                IDirectoryInfo parent = localFileInfo.Directory;
                IMappedObject mappedParent = storage.GetObjectByLocalPath(parent);

                Dictionary<string, object> properties = new Dictionary<string, object>();
                properties.Add(PropertyIds.Name, localFileInfo.Name);
                properties.Add(PropertyIds.ObjectTypeId, "cmis:document");
                IDocument doc = session.GetObject(session.CreateDocument(properties, new ObjectId(mappedParent.RemoteObjectId), null, null, null, null, null)) as IDocument;
                Guid uuid = Guid.Empty;
                if (localFileInfo.IsExtendedAttributeAvailable()) {
                    uuid = Guid.NewGuid();
                    localFileInfo.SetExtendedAttribute(MappedObject.ExtendedAttributeKey, uuid.ToString());
                }

                localFileInfo.LastWriteTimeUtc = doc.LastModificationDate != null ? (DateTime)doc.LastModificationDate : localFileInfo.LastWriteTimeUtc;

                IMappedObject mappedFile = new MappedObject(
                    localFileInfo.Name,
                    doc.Id,
                    MappedObjectType.File,
                    mappedParent.RemoteObjectId,
                    doc.ChangeToken)
                {
                    Guid = uuid,
                    LastRemoteWriteTimeUtc = doc.LastModificationDate,
                    LastLocalWriteTimeUtc = localFileInfo.LastWriteTimeUtc
                };
                storage.SaveMappedObject(mappedFile);
            }
        }
    }
}