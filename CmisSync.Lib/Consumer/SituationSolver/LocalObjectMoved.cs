//-----------------------------------------------------------------------
// <copyright file="LocalObjectMoved.cs" company="GRAU DATA AG">
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

    using log4net;

    /// <summary>
    /// A Local object has been moved. => Move the corresponding object on the server.
    /// </summary>
    public class LocalObjectMoved : ISolver
    {
        private static readonly ILog OperationsLogger = LogManager.GetLogger("OperationsLogger");

        /// <summary>
        /// Solve the specified situation by using the session, storage, localFile and remoteId.
        /// </summary>
        /// <param name="session">Cmis session instance.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="localFile">Actual local file.</param>
        /// <param name="remoteId">Corresponding remote identifier.</param>
        public virtual void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFile, IObjectId remoteId)
        {
            // Move Remote Object
            var remoteObject = remoteId as IFileableCmisObject;
            var mappedObject = storage.GetObjectByRemoteId(remoteId.Id);
            var targetPath = localFile is IDirectoryInfo ? (localFile as IDirectoryInfo).Parent : (localFile as IFileInfo).Directory;
            var targetId = storage.GetObjectByLocalPath(targetPath).RemoteObjectId;
            var src = session.GetObject(mappedObject.ParentId);
            var target = session.GetObject(targetId);
            OperationsLogger.Info(string.Format("Moving remote object {2} from folder {0} to folder {1}", src.Name, target.Name, remoteId.Id));
            remoteObject = remoteObject.Move(src, target);
            if(localFile.Name != remoteObject.Name) {
                remoteObject.Rename(localFile.Name, true);
            }

            mappedObject.ParentId = targetId;
            mappedObject.LastChangeToken = remoteObject.ChangeToken;
            mappedObject.LastRemoteWriteTimeUtc = remoteObject.LastModificationDate;
            mappedObject.Name = remoteObject.Name;
            storage.SaveMappedObject(mappedObject);
        }
    }
}
