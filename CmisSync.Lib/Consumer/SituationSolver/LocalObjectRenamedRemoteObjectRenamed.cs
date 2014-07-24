//-----------------------------------------------------------------------
// <copyright file="LocalObjectRenamedRemoteObjectRenamed.cs" company="GRAU DATA AG">
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
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using log4net;

    /// <summary>
    /// Local object renamed and also the remote object has been renamed.
    /// </summary>
    public class LocalObjectRenamedRemoteObjectRenamed : AbstractEnhancedSolver
    {
        private static readonly ILog OperationsLogger = LogManager.GetLogger("OperationsLogger");

        public LocalObjectRenamedRemoteObjectRenamed(ISession session, IMetaDataStorage storage) : base(session, storage) {
        }

        /// <summary>
        /// Solve the specified situation by taking renaming the local or remote object to the name of the last changed object.
        /// </summary>
        /// <param name="localFile">Local file.</param>
        /// <param name="remoteId">Remote object.</param>
        public override void Solve(
            IFileSystemInfo localFile,
            IObjectId remoteId,
            ContentChangeType localContent = ContentChangeType.NONE,
            ContentChangeType remoteContent = ContentChangeType.NONE)
        {
            if (localFile is IDirectoryInfo) {
                var localFolder = localFile as IDirectoryInfo;
                var remoteFolder = remoteId as IFolder;
                var mappedObject = this.Storage.GetObjectByRemoteId(remoteFolder.Id);
                if (localFolder.Name.Equals(remoteFolder.Name)) {
                    mappedObject.Name = localFolder.Name;
                } else if (localFolder.LastWriteTimeUtc.CompareTo((DateTime)remoteFolder.LastModificationDate) > 0) {
                    string oldName = remoteFolder.Name;
                    remoteFolder.Rename(localFolder.Name, true);
                    mappedObject.Name = remoteFolder.Name;
                    OperationsLogger.Info(string.Format("Renamed remote folder {0} with id {2} to {1}", oldName, remoteFolder.Id, remoteFolder.Name));
                } else {
                    string oldName = localFolder.Name;
                    localFolder.MoveTo(Path.Combine(localFolder.Parent.FullName, remoteFolder.Name));
                    mappedObject.Name = remoteFolder.Name;
                    OperationsLogger.Info(string.Format("Renamed local folder {0} to {1}", Path.Combine(localFolder.Parent.FullName, oldName), remoteFolder.Name));
                }

                mappedObject.LastLocalWriteTimeUtc = localFolder.LastWriteTimeUtc;
                mappedObject.LastRemoteWriteTimeUtc = (DateTime)remoteFolder.LastModificationDate;
                mappedObject.LastChangeToken = remoteFolder.ChangeToken;
                this.Storage.SaveMappedObject(mappedObject);
            } else {
                throw new NotImplementedException("File scenario is not implemented yet");
            }
        }
    }
}