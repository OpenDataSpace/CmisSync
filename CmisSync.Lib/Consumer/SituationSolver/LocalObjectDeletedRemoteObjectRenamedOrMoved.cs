//-----------------------------------------------------------------------
// <copyright file="LocalObjectDeletedRemoteObjectRenamedOrMoved.cs" company="GRAU DATA AG">
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

    public class LocalObjectDeletedRemoteObjectRenamedOrMoved : AbstractEnhancedSolver
    {
        private ISolver secondSolver;

        public LocalObjectDeletedRemoteObjectRenamedOrMoved(
            ISession session,
            IMetaDataStorage storage,
            ActiveActivitiesManager manager,
            bool serverCanModifyDates,
            ISolver secondSolver = null) : base(session, storage, serverCanModifyDates) {
            this.secondSolver = secondSolver ?? new LocalObjectAdded(session, storage, manager, serverCanModifyDates);
        }

        public override void Solve(
            IFileSystemInfo localFile,
            IObjectId remoteId,
            ContentChangeType localContent = ContentChangeType.NONE,
            ContentChangeType remoteContent = ContentChangeType.NONE)
        {
            throw new NotImplementedException();
        }
    }
}