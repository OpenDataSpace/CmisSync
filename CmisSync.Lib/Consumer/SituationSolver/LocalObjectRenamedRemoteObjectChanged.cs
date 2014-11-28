//-----------------------------------------------------------------------
// <copyright file="LocalObjectRenamedRemoteObjectChanged.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    public class LocalObjectRenamedRemoteObjectChanged : AbstractEnhancedSolver
    {
        private LocalObjectChangedRemoteObjectChanged changeChangeSolver;

        public LocalObjectRenamedRemoteObjectChanged(
            ISession session,
            IMetaDataStorage storage,
            LocalObjectChangedRemoteObjectChanged changeSolver) : base(session, storage) {
            if (changeSolver == null) {
                throw new ArgumentNullException("Given situation solver for local and remote changes is null");
            }

            this.changeChangeSolver = changeSolver;
        }

        public override void Solve(
            IFileSystemInfo localFileSystemInfo,
            IObjectId remoteId, 
            ContentChangeType localContent,
            ContentChangeType remoteContent)
        {
            var obj = this.Storage.GetObjectByRemoteId(remoteId.Id);
            string oldName = (remoteId as ICmisObject).Name;

            // Rename object
            try {
                (remoteId as ICmisObject).Rename(localFileSystemInfo.Name, true);
            } catch (CmisPermissionDeniedException) {
                OperationsLogger.Warn(string.Format("Unable to renamed remote object from {0} to {1}: Permission Denied", oldName, localFileSystemInfo.Name));
                return;
            }

            obj.Name = localFileSystemInfo.Name;
            this.Storage.SaveMappedObject(obj);
            this.changeChangeSolver.Solve(localFileSystemInfo, remoteId, localContent, remoteContent);
        }
    }
}