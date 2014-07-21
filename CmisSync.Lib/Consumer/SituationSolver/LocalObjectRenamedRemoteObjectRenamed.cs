//-----------------------------------------------------------------------
// <copyright file="LocalObjectRenamedRemoteObjectRenamed.cs" company="GRAU DATA AG">
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
using System.IO;

namespace CmisSync.Lib.Consumer.SituationSolver
{
    using System;

    using CmisSync.Lib.Storage.FileSystem;
    using CmisSync.Lib.Storage.Database;

    using DotCMIS.Client;

    public class LocalObjectRenamedRemoteObjectRenamed : ISolver
    {
        public virtual void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFile, IObjectId remoteId)
        {
            if (localFile is IDirectoryInfo) {
                var localFolder = localFile as IDirectoryInfo;
                var remoteFolder = remoteId as IFolder;
                var mappedObject = storage.GetObjectByRemoteId(remoteFolder.Id);
                if (localFolder.Name.Equals(remoteFolder.Name)) {
                    mappedObject.Name = localFolder.Name;
                } else if (localFolder.LastWriteTimeUtc.CompareTo((DateTime)remoteFolder.LastModificationDate) > 0 ) {
                    remoteFolder.Rename(localFolder.Name, true);
                    mappedObject.Name = remoteFolder.Name;
                } else {
                    localFolder.MoveTo(Path.Combine(localFolder.Parent.FullName, remoteFolder.Name));
                    mappedObject.Name = remoteFolder.Name;
                }

                mappedObject.LastChangeToken = remoteFolder.ChangeToken;
                storage.SaveMappedObject(mappedObject);
            } else {
                throw new NotImplementedException("File scenario is not implemented yet");
            }
        }
    }
}