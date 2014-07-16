//-----------------------------------------------------------------------
// <copyright file="RemoteObjectDeleted.cs" company="GRAU DATA AG">
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
    /// Remote object has been deleted. => Delete the corresponding local object as well.
    /// </summary>
    public class RemoteObjectDeleted : ISolver
    {
        private static readonly ILog OperationsLogger = LogManager.GetLogger("OperationsLogger");

        /// <summary>
        /// Deletes the given localFileInfo on file system and removes the stored object from storage.
        /// </summary>
        /// <param name="session">Cmis session instance.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="localFileInfo">Local file info.</param>
        /// <param name="remoteId">Remote identifier.</param>
        public virtual void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFileInfo, IObjectId remoteId)
        {
            if (localFileInfo is IDirectoryInfo) {
                if (!this.DeleteLocalObjectIfHasBeenSyncedBefore(storage, localFileInfo)) {
                    storage.RemoveObject(storage.GetObjectByLocalPath(localFileInfo));
                    throw new IOException(string.Format("Not all local objects under {0} have been synced yet", localFileInfo.FullName));
                } else {
                    storage.RemoveObject(storage.GetObjectByLocalPath(localFileInfo));
                }
            } else if (localFileInfo is IFileInfo) {
                var file = localFileInfo as IFileInfo;
                var mappedFile = storage.GetObjectByLocalPath(file);
                if (mappedFile != null && file.LastWriteTimeUtc.Equals(mappedFile.LastLocalWriteTimeUtc)) {
                    file.Delete();
                    OperationsLogger.Info(string.Format("Deleted local file {0} because the mapped remote object {0} has been deleted", file.FullName, mappedFile.RemoteObjectId));
                } else {
                    file.SetExtendedAttribute(MappedObject.ExtendedAttributeKey, null);
                    if (mappedFile == null) {
                        return;
                    }

                    OperationsLogger.Info(string.Format("Deletion of local file {0} skipped because of not yet uploaded changes", file.FullName));
                }

                storage.RemoveObject(storage.GetObjectByLocalPath(localFileInfo));
            }
        }

        private bool DeleteLocalObjectIfHasBeenSyncedBefore(IMetaDataStorage storage, IFileSystemInfo fsInfo) {
            bool delete = true;

            Guid uuid;
            IMappedObject obj = null;
            if (Guid.TryParse(fsInfo.GetExtendedAttribute(MappedObject.ExtendedAttributeKey), out uuid)) {
                obj = storage.GetObjectByGuid(uuid);
            } else {
                obj = storage.GetObjectByLocalPath(fsInfo);
            }

            if (fsInfo is IFileInfo) {
                if (obj != null && fsInfo.LastWriteTimeUtc.Equals(obj.LastLocalWriteTimeUtc)) {
                    (fsInfo as IFileInfo).Delete();
                    OperationsLogger.Info(string.Format("Deleted local file {0} because the mapped remote object {0} has been deleted", fsInfo.FullName, obj.RemoteObjectId));
                } else {
                    fsInfo.SetExtendedAttribute(MappedObject.ExtendedAttributeKey, null);
                    return false;
                }
            } else if (fsInfo is IDirectoryInfo) {
                var dir = fsInfo as IDirectoryInfo;
                foreach (var folder in dir.GetDirectories()) {
                    if (!this.DeleteLocalObjectIfHasBeenSyncedBefore(storage, folder)) {
                        delete = false;
                    }
                }

                foreach (var file in dir.GetFiles()) {
                    if (!this.DeleteLocalObjectIfHasBeenSyncedBefore(storage, file)) {
                        delete = false;
                    }
                }

                if (delete) {
                    try {
                        (fsInfo as IDirectoryInfo).Delete(false);
                        OperationsLogger.Info(string.Format("Deleted local folder {0} because the mapped remote folder has been deleted", fsInfo.FullName));
                    } catch (IOException) {
                        fsInfo.SetExtendedAttribute(MappedObject.ExtendedAttributeKey, null);
                        return false;
                    }
                } else {
                    fsInfo.SetExtendedAttribute(MappedObject.ExtendedAttributeKey, null);
                }
            }

            return delete;
        }
    }
}
