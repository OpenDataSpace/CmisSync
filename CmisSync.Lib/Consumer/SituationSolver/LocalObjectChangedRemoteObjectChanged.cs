//-----------------------------------------------------------------------
// <copyright file="LocalObjectChangedRemoteObjectChanged.cs" company="GRAU DATA AG">
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
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    /// <summary>
    /// Local object changed and remote object changed.
    /// </summary>
    public class LocalObjectChangedRemoteObjectChanged : AbstractEnhancedSolver
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="CmisSync.Lib.Consumer.SituationSolver.LocalObjectChangedRemoteObjectChanged"/> class.
        /// </summary>
        /// <param name="session">Cmis session.</param>
        /// <param name="storage">Meta data storage.</param>
        public LocalObjectChangedRemoteObjectChanged(ISession session, IMetaDataStorage storage) : base(session, storage) {
        }

        public override void Solve(
            IFileSystemInfo localFileSystemInfo,
            IObjectId remoteId,
            ContentChangeType localContent,
            ContentChangeType remoteContent)
        {
            if (localFileSystemInfo is IDirectoryInfo) {
                var obj = this.Storage.GetObjectByRemoteId((remoteId as IFolder).Id);
                obj.LastLocalWriteTimeUtc = localFileSystemInfo.LastWriteTimeUtc;
                obj.LastRemoteWriteTimeUtc = (remoteId as IFolder).LastModificationDate;
                obj.LastChangeToken = (remoteId as IFolder).ChangeToken;
                this.Storage.SaveMappedObject(obj);
            } else {
                throw new NotImplementedException();
            }
        }
    }
}