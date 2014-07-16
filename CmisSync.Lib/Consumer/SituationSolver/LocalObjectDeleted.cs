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

namespace CmisSync.Lib.Consumer.SituationSolver
{
    using System;
    using System.IO;

    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Storage.Database;

    using DotCMIS.Client;
    using DotCMIS.Enums;
    using DotCMIS.Exceptions;

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
            var mappedObject = storage.GetObjectByRemoteId(remoteId.Id);

            bool hasBeenDeleted = TryDeleteObjectOnServer(session, remoteId, mappedObject.Type);
            if(hasBeenDeleted) {
                storage.RemoveObject(mappedObject);
                OperationsLogger.Info(string.Format("Deleted the corresponding remote object {0} of locally deleted object {1}", remoteId.Id, mappedObject.Name));
            } else {
                OperationsLogger.Warn(string.Format("Permission denied while trying to Delete the locally deleted object {0} on the server.", mappedObject.Name));
            }

        }

        private bool TryDeleteObjectOnServer(ISession session, IObjectId remoteId, MappedObjectType type)
        {
            try{
                if (type == MappedObjectType.Folder) {
                    (remoteId as IFolder).DeleteTree(false, UnfileObject.DeleteSinglefiled, true);
                } else {
                    session.Delete(remoteId, true);
                }
            } catch (CmisPermissionDeniedException){
                return false;
            }
            return true;
        }
    }
}
