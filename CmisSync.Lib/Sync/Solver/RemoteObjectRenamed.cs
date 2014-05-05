//-----------------------------------------------------------------------
// <copyright file="RemoteObjectRenamed.cs" company="GRAU DATA AG">
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
    /// Remote object has been renamed. => Rename the corresponding local object.
    /// </summary>
    public class RemoteObjectRenamed : ISolver
    {
        public virtual void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFile, IObjectId remoteId)
        {
            // Rename local folder
            if(remoteId is IFolder)
            {
                IFolder remoteFolder = remoteId as IFolder;
                IDirectoryInfo dirInfo = localFile as IDirectoryInfo;
                IMappedObject obj = storage.GetObjectByRemoteId(remoteFolder.Id);
                dirInfo.MoveTo(Path.Combine(dirInfo.Parent.FullName, remoteFolder.Name));
                obj.Name = remoteFolder.Name;
                obj.LastChangeToken = remoteFolder.ChangeToken;
                obj.LastRemoteWriteTimeUtc = remoteFolder.LastModificationDate;
                storage.SaveMappedObject(obj);
            }
            else if(remoteId is IDocument)
            {
                throw new NotImplementedException();
            }
        }
    }
}