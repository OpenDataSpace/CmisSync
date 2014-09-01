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
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    /// <summary>
    /// Local object deleted and the corresponding remote object is renamed or moved.
    /// </summary>
    public class LocalObjectDeletedRemoteObjectRenamedOrMoved : AbstractEnhancedSolver
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="CmisSync.Lib.Consumer.SituationSolver.LocalObjectDeletedRemoteObjectRenamedOrMoved"/> class.
        /// </summary>
        /// <param name="session">Cmis session.</param>
        /// <param name="storage">Meta data storage.</param>
        public LocalObjectDeletedRemoteObjectRenamedOrMoved(
            ISession session,
            IMetaDataStorage storage) : base(session, storage) {
        }

        /// <summary>
        /// Solve the specified situation by using the storage and remote object id to remove existing db entries and forces a crawl sync by throwing an IOException.
        /// </summary>
        /// <param name="localFile">Deleted Local filesystem info instance.</param>
        /// <param name="remoteId">Remote identifier or object.</param>
        /// <param name="localContent">Hint if the local content has been changed. Is not used by this solver.</param>
        /// <param name="remoteContent">Information if the remote content has been changed. Is not used by this solver.</param>
        public override void Solve(
            IFileSystemInfo localFile,
            IObjectId remoteId,
            ContentChangeType localContent = ContentChangeType.NONE,
            ContentChangeType remoteContent = ContentChangeType.NONE)
        {
            var mappedObject = this.Storage.GetObjectByRemoteId(remoteId.Id);
            this.Storage.RemoveObject(mappedObject);
            throw new IOException(
                string.Format(
                "Local deleted {0} is renamed or moved remotely => invoking crawl sync to download them again",
                mappedObject.Type == MappedObjectType.File ? "file" : "directory"));
        }
    }
}