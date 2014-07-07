//-----------------------------------------------------------------------
// <copyright file="LocalObjectDeleted.cs" company="GRAU DATA AG">
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

    using log4net;

    /// <summary>
    /// A Local object has been deleted. => Delete the corresponding object on the server, if possible
    /// </summary>
    public class LocalObjectDeleted : ISolver
    {
        private static readonly ILog OperationsLogger = LogManager.GetLogger("OperationsLogger");

        /// <summary>
        /// Solves the situation by deleting the corresponding remote object.
        /// </summary>
        /// <param name="session">Cmis session instance.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="localFile">Local file.</param>
        /// <param name="remoteId">Remote identifier.</param>
        public virtual void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFile, IObjectId remoteId)
        {
            string id = remoteId.Id;
            var mappedObject = storage.GetObjectByRemoteId(id);
            if (mappedObject.Type == MappedObjectType.Folder) {
                (remoteId as IFolder).DeleteTree(true, null, true);
            } else {
                session.Delete(remoteId, true);
            }

            storage.RemoveObject(mappedObject);
            OperationsLogger.Info(string.Format("Deleted the corresponding remote object {0} of locally deleted object {1}", remoteId.Id, mappedObject.Name));
        }
    }
}