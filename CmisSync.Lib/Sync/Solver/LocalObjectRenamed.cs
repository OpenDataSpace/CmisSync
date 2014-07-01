//-----------------------------------------------------------------------
// <copyright file="LocalObjectRenamed.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;

    using DotCMIS.Client;

    using log4net;

    /// <summary>
    /// Local object has been renamed. => Rename the corresponding object on the server.
    /// </summary>
    public class LocalObjectRenamed : ISolver
    {
        private static readonly ILog OperationsLogger = LogManager.GetLogger("OperationsLogger");

        /// <summary>
        /// Solve the specified situation by using the session, storage, localFile and remoteId.
        /// </summary>
        /// <param name="session">Cmis session instance.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="localFile">Local file.</param>
        /// <param name="remoteId">Remote identifier.</param>
        public virtual void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFile, IObjectId remoteId)
        {
            var obj = storage.GetObjectByRemoteId(remoteId.Id);
            ICmisObject remoteObject;

            // Rename remote object
            if(remoteId is IFolder) {
                string oldName = (remoteId as IFolder).Name;
                remoteObject = (remoteId as IFolder).Rename(localFile.Name, true) as IFolder;
                OperationsLogger.Info(string.Format("Renamed remote folder {0} from {1} to {2}", remoteObject.Id, oldName, localFile.Name));
            } else if (remoteId is IDocument) {
                string oldName = (remoteId as IDocument).Name;
                remoteObject = (remoteId as IDocument).Rename(localFile.Name, true) as IDocument;
                OperationsLogger.Info(string.Format("Renamed remote document {0} from {1} to {2}", remoteObject.Id, oldName, localFile.Name));
            } else {
                throw new ArgumentException("Given remoteId type is unknown: " + remoteId.GetType().Name);
            }

            localFile.LastWriteTimeUtc = remoteObject.LastModificationDate != null ? (DateTime)remoteObject.LastModificationDate : localFile.LastWriteTimeUtc;
            obj.Name = remoteObject.Name;
            obj.LastRemoteWriteTimeUtc = remoteObject.LastModificationDate;
            obj.LastLocalWriteTimeUtc = localFile.LastWriteTimeUtc;
            obj.LastChangeToken = remoteObject.ChangeToken;
            storage.SaveMappedObject(obj);
        }
    }
}