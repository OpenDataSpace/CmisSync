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
        public void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFile, IObjectId remoteId)
        {
            string parentId = this.GetParentId(localFile, storage);
            ICmisObject addedObject = this.AddCmisObject(localFile, parentId, session);

            Guid uuid = this.WriteUuidToExtendedAttribute(localFile);

            localFile.LastWriteTimeUtc = addedObject.LastModificationDate != null ? (DateTime)addedObject.LastModificationDate : localFile.LastWriteTimeUtc;

            MappedObject mappedFolder = new MappedObject(
                    localFile.Name,
                    addedObject.Id,
                    localFile is IDirectoryInfo ? MappedObjectType.Folder : MappedObjectType.File,
                    parentId,
                    addedObject.ChangeToken)
                {
                    Guid = uuid,
                    LastRemoteWriteTimeUtc = addedObject.LastModificationDate,
                    LastLocalWriteTimeUtc = localFile.LastWriteTimeUtc
                };
            storage.SaveMappedObject(mappedFolder);
        }

        private static Guid WriteUuidToExtendedAttribute(IFileSystemInfo localFile)
        {
            Guid uuid = Guid.Empty;
            if (localFile.IsExtendedAttributeAvailable())
            {
                uuid = Guid.NewGuid();
                localFile.SetExtendedAttribute(MappedObject.ExtendedAttributeKey, uuid.ToString());
            }
            
            return uuid;
        }

        private string GetParentId(IFileSystemInfo fileInfo, IMetaDataStorage storage)
        {
            IDirectoryInfo parent = null;
            if (fileInfo is IDirectoryInfo) {
                IDirectoryInfo localDirInfo = fileInfo as IDirectoryInfo;
                parent = localDirInfo.Parent;
            } else {
                IFileInfo localFileInfo = fileInfo as IFileInfo;
                parent = localFileInfo.Directory;
            }

            IMappedObject mappedParent = storage.GetObjectByLocalPath(parent);
            return mappedParent.RemoteObjectId;
        }

        private ICmisObject AddCmisObject(IFileSystemInfo localFile, string parentId, ISession session)
        {
            string name = localFile.Name;
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, name);
            if (localFile is IDirectoryInfo) {
                properties.Add(PropertyIds.ObjectTypeId, "cmis:folder");
                return session.GetObject(session.CreateFolder(properties, new ObjectId(parentId))) as IFolder;
            } else {
                properties.Add(PropertyIds.ObjectTypeId, "cmis:document");
                return session.GetObject(session.CreateDocument(properties, new ObjectId(parentId), null, null, null, null, null)) as IDocument;
            }
        }
    }
}