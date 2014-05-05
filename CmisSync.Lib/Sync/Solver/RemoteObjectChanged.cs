//-----------------------------------------------------------------------
// <copyright file="RemoteObjectChanged.cs" company="GRAU DATA AG">
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
    /// Remote object has been changed. => update the metadata locally.
    /// </summary>
    public class RemoteObjectChanged : ISolver
    {
        public virtual void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFile, IObjectId remoteId)
        {
            // NOOP at the moment
            /*IMappedObject obj = storage.GetObjectByRemoteId(remoteId.Id);
            DateTime? lastModified = remoteId is IFolder ? (remoteId as IFolder).LastModificationDate : (remoteId as IDocument).LastModificationDate;
            obj.LastRemoteWriteTimeUtc = lastModified;
            if(lastModified != null) {
                localFile.LastWriteTimeUtc = (DateTime)lastModified;
            }

            storage.SaveMappedObject(obj);*/
        }
    }
}