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
    using CmisSync.Lib.Storage.FileSystem;
    using CmisSync.Lib.Storage.Database;

    using DotCMIS.Client;
    using DotCMIS.Enums;
    using DotCMIS.Exceptions;

    using log4net;

    /// <summary>
    /// A Local object has been deleted. => Delete the corresponding object on the server, if possible
    /// </summary>
    public class LocalObjectDeleted : AbstractEnhancedSolver
    {
        private static readonly ILog OperationsLogger = LogManager.GetLogger("OperationsLogger");

        public LocalObjectDeleted(ISession session, IMetaDataStorage storage) : base (session, storage) {
        }

        /// <summary>
        /// Solves the situation by deleting the corresponding remote object.
        /// </summary>
        /// <param name="localFile">Local file.</param>
        /// <param name="remoteId">Remote identifier.</param>
        public override void Solve(
            IFileSystemInfo localFile,
            IObjectId remoteId,
            ContentChangeType localContent = ContentChangeType.NONE,
            ContentChangeType remoteContent = ContentChangeType.NONE)
        {
            var mappedObject = this.Storage.GetObjectByRemoteId(remoteId.Id);

            bool hasBeenDeleted = TryDeleteObjectOnServer(remoteId, mappedObject.Type);
            if(hasBeenDeleted) {
                this.Storage.RemoveObject(mappedObject);
                OperationsLogger.Info(string.Format("Deleted the corresponding remote object {0} of locally deleted object {1}", remoteId.Id, mappedObject.Name));
            } else {
                OperationsLogger.Warn(string.Format("Permission denied while trying to Delete the locally deleted object {0} on the server.", mappedObject.Name));
            }

        }

        private bool TryDeleteObjectOnServer(IObjectId remoteId, MappedObjectType type)
        {
            try{
                if (type == MappedObjectType.Folder) {
                    (remoteId as IFolder).DeleteTree(false, UnfileObject.DeleteSinglefiled, true);
                } else {
                    this.Session.Delete(remoteId, true);
                }
            } catch (CmisPermissionDeniedException){
                return false;
            }
            return true;
        }
    }
}
