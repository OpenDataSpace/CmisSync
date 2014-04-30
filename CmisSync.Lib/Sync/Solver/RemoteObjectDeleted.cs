//-----------------------------------------------------------------------
// <copyright file="RemoteObjectDeleted.cs" company="GRAU DATA AG">
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
    /// Remote object has been deleted. => Delete the corresponding local object as well.
    /// </summary>
    public class RemoteObjectDeleted : ISolver
    {
        public virtual void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFileInfo, IObjectId remoteId)
        {
            if(localFileInfo is IDirectoryInfo)
            {
                var localFolder = localFileInfo as IDirectoryInfo;
                localFolder.Delete(true);
            }

            storage.RemoveObject(storage.GetObjectByLocalPath(localFileInfo));
        }
    }
}
