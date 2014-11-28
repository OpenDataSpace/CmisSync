//-----------------------------------------------------------------------
// <copyright file="LocalObjectRenamedOrMovedRemoteObjectDeleted.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Consumer.SituationSolver
{
    using System;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    /// <summary>
    /// Local object renamed or moved and the corresponding remote object is deleted.
    /// </summary>
    public class LocalObjectRenamedOrMovedRemoteObjectDeleted : AbstractEnhancedSolver
    {
        private ISolver secondSolver;

        public LocalObjectRenamedOrMovedRemoteObjectDeleted(
            ISession session,
            IMetaDataStorage storage,
            ActiveActivitiesManager manager,
            ISolver secondSolver = null) : base(session, storage) {
            this.secondSolver = secondSolver ?? new LocalObjectAdded(session, storage, manager);
        }

        public override void Solve(
            IFileSystemInfo localFileSystemInfo,
            IObjectId remoteId,
            ContentChangeType localContent,
            ContentChangeType remoteContent)
        {
            var mappedObject = this.Storage.GetObjectByGuid((Guid)localFileSystemInfo.Uuid);
            this.Storage.RemoveObject(mappedObject);
            if (localFileSystemInfo is IFileInfo) {
                this.secondSolver.Solve(localFileSystemInfo, null, ContentChangeType.CREATED, ContentChangeType.NONE);
            } else if (localFileSystemInfo is IDirectoryInfo) {
                this.secondSolver.Solve(localFileSystemInfo, null, ContentChangeType.NONE, ContentChangeType.NONE);
                var dir = localFileSystemInfo as IDirectoryInfo;
                if (dir.GetFiles().Length > 0 || dir.GetDirectories().Length > 0) {
                    throw new IOException(string.Format("There are unsynced files in local folder {0} => starting crawl sync", dir.FullName));
                }
            }
        }
    }
}