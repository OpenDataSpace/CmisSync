//-----------------------------------------------------------------------
// <copyright file="RemoteObjectMoved.cs" company="GRAU DATA AG">
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
    using System.IO;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;

    using DotCMIS.Client;

    /// <summary>
    /// Remote object has been moved. => Move the corresponding local object.
    /// </summary>
    public class RemoteObjectMoved : ISolver
    {
        /// <summary>
        /// Solve the specified situation by using the session, storage, localFile and remoteId.
        /// Moves the local file/folder to the new location.
        /// </summary>
        /// <param name="session">Cmis session instance.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="localFile">Old local file/folder.</param>
        /// <param name="remoteId">Remote identifier.</param>
        public virtual void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFile, IObjectId remoteId)
        {
            // Move local object
            var savedObject = storage.GetObjectByRemoteId(remoteId.Id);
            string newPath = remoteId is IFolder ? storage.Matcher.CreateLocalPath(remoteId as IFolder) : storage.Matcher.CreateLocalPath(remoteId as IDocument);
            if (remoteId is IFolder) {
                IDirectoryInfo dirInfo = localFile as IDirectoryInfo;
                dirInfo.MoveTo(newPath);
                dirInfo.LastWriteTimeUtc = (remoteId as IFolder).LastModificationDate != null ? (DateTime)(remoteId as IFolder).LastModificationDate : dirInfo.LastWriteTimeUtc;
            } else if (remoteId is IDocument) {
                IFileInfo fileInfo = localFile as IFileInfo;
                fileInfo.MoveTo(newPath);
                fileInfo.LastWriteTimeUtc = (remoteId as IDocument).LastModificationDate != null ? (DateTime)(remoteId as IDocument).LastModificationDate : fileInfo.LastWriteTimeUtc;
            }

            savedObject.Name = (remoteId as ICmisObject).Name;
            savedObject.ParentId = remoteId is IFolder ? (remoteId as IFolder).ParentId : (remoteId as IDocument).Parents[0].Id;
            savedObject.LastChangeToken = remoteId is ICmisObject ? (remoteId as ICmisObject).ChangeToken : null;
            savedObject.LastLocalWriteTimeUtc = localFile.LastWriteTimeUtc;
            savedObject.LastRemoteWriteTimeUtc = (remoteId as ICmisObject).LastModificationDate;
            storage.SaveMappedObject(savedObject);
        }
    }
}