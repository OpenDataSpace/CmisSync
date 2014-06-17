//-----------------------------------------------------------------------
// <copyright file="LocalObjectChanged.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;

    using DotCMIS.Client;

    /// <summary>
    /// A local object has been changed and should be uploaded (if necessary) to server or updated on the server.
    /// </summary>
    public class LocalObjectChanged : ISolver
    {
        public virtual void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFileSystemInfo, IObjectId remoteId)
        {
            // Match local changes to remote changes and updated them remotely
            var mappedObject = storage.GetObjectByLocalPath(localFileSystemInfo);
            var localFile = localFileSystemInfo as IFileInfo;
            mappedObject.LastLocalWriteTimeUtc = localFileSystemInfo.LastWriteTimeUtc;
            storage.SaveMappedObject(mappedObject);
        }
    }
}
