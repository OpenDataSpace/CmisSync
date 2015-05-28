//-----------------------------------------------------------------------
// <copyright file="LocalObjectMovedRemoteObjectChanged.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Consumer.SituationSolver {
    using System;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    public class LocalObjectMovedRemoteObjectChanged : AbstractEnhancedSolver {
        private readonly ISolver renameChangeSolver;
        private readonly ISolver changeChangeSolver;
        public LocalObjectMovedRemoteObjectChanged(
            ISession session,
            IMetaDataStorage storage,
            ISolver renameChangeSolver,
            ISolver changeChangeSolver) : base(session, storage) {
            if (renameChangeSolver == null) {
                throw new ArgumentNullException("renameChangeSolver", "Given sitation solver for local rename and remote change is null");
            }

            if (changeChangeSolver == null) {
                throw new ArgumentNullException("changeChangeSolver", "Given situation solver for local and remote changes is null");
            }

            this.renameChangeSolver = renameChangeSolver;
            this.changeChangeSolver = changeChangeSolver;
        }

        public override void Solve(
            IFileSystemInfo localFileSystemInfo,
            IObjectId remoteId,
            ContentChangeType localContent,
            ContentChangeType remoteContent)
        {
            var obj = this.Storage.GetObjectByRemoteId(remoteId.Id);
            var localParent = localFileSystemInfo is IFileInfo ? (localFileSystemInfo as IFileInfo).Directory : (localFileSystemInfo as IDirectoryInfo).Parent;
            var mappedLocalParent = this.Storage.GetObjectByGuid((Guid)localParent.Uuid);
            var remoteObject = remoteId as IFileableCmisObject;
            var targetId = mappedLocalParent.RemoteObjectId;
            var src = this.Session.GetObject(obj.ParentId);
            var target = this.Session.GetObject(targetId);
            try {
                OperationsLogger.Info(string.Format("Moving remote object {2} from folder {0} to folder {1}", src.Name, target.Name, remoteId.Id));
                remoteObject = remoteObject.Move(src, target);
            } catch (CmisPermissionDeniedException) {
                OperationsLogger.Info(string.Format("Moving remote object failed {0}: Permission Denied", localFileSystemInfo.FullName));
                return;
            }

            obj.ParentId = targetId;
            obj.Ignored = remoteObject.AreAllChildrenIgnored();
            this.Storage.SaveMappedObject(obj);

            if (obj.Name != localFileSystemInfo.Name) {
                this.renameChangeSolver.Solve(localFileSystemInfo, remoteObject, localContent, remoteContent);
            } else {
                this.changeChangeSolver.Solve(localFileSystemInfo, remoteObject, localContent, remoteContent);
            }
        }
    }
}