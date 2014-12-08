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

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    /// <summary>
    /// A Local object has been moved. => Move the corresponding object on the server.
    /// </summary>
    public class LocalObjectMoved : AbstractEnhancedSolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Consumer.SituationSolver.LocalObjectMoved"/> class.
        /// </summary>
        /// <param name="session">Cmis session.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="serverCanModifyCreationAndModificationDate">If set to <c>true</c> server can modify creation and modification date.</param>
        public LocalObjectMoved(
            ISession session,
            IMetaDataStorage storage,
            ISyncEventQueue queue) : base(session, storage, queue)
        {
            if (this.Queue == null) {
                throw new ArgumentNullException("Given queue is null");
            }
        }

        /// <summary>
        /// Solve the specified situation by using the session, storage, localFile and remoteId.
        /// </summary>
        /// <param name="localFile">Actual local file.</param>
        /// <param name="remoteId">Corresponding remote identifier.</param>
        /// <param name="localContent">Hint if the local content has been changed.</param>
        /// <param name="remoteContent">Information if the remote content has been changed.</param>
        public override void Solve(
            IFileSystemInfo localFile,
            IObjectId remoteId,
            ContentChangeType localContent = ContentChangeType.NONE,
            ContentChangeType remoteContent = ContentChangeType.NONE)
        {
            // Move Remote Object
            var remoteObject = remoteId as IFileableCmisObject;
            var mappedObject = this.Storage.GetObjectByRemoteId(remoteId.Id);

            if (mappedObject.LastChangeToken != (remoteId as ICmisObject).ChangeToken) {
                throw new ArgumentException("The remote change token is different to the last synchronization");
            }

            var targetPath = localFile is IDirectoryInfo ? (localFile as IDirectoryInfo).Parent : (localFile as IFileInfo).Directory;
            var targetId = this.Storage.GetObjectByLocalPath(targetPath).RemoteObjectId;
            try {
                if (mappedObject.ParentId != targetId) {
                    var src = this.Session.GetObject(mappedObject.ParentId);
                    var target = this.Session.GetObject(targetId);
                    OperationsLogger.Info(string.Format("Moving remote object {2} from folder {0} to folder {1}", src.Name, target.Name, remoteId.Id));
                    remoteObject = remoteObject.Move(src, target);
                }

                if (localFile.Name != remoteObject.Name) {
                    try {
                        remoteObject.Rename(localFile.Name, true);
                    } catch (CmisConstraintException e) {
                        if (!Utils.IsValidISO885915(localFile.Name)) {
                            this.Queue.AddEvent(new InteractionNeededEvent(e) {
                                Title = string.Format("Server denied renaming of {0}", remoteObject.Name),
                                Description = string.Format("Server denied the rename of {0} to {1}, possibly because it contains UTF-8 charactes", remoteObject.Name, localFile.Name)
                            });
                            OperationsLogger.Warn(string.Format("Server denied the rename of {0} to {1}, possibly because it contains UTF-8 charactes", remoteObject.Name, localFile.Name));
                            return;
                        }

                        throw;
                    }
                }
            } catch (CmisPermissionDeniedException) {
                OperationsLogger.Info(string.Format("Moving remote object failed {0}: Permission Denied", localFile.FullName));
                return;
            }

            if (this.ServerCanModifyDateTimes) {
                if (mappedObject.LastLocalWriteTimeUtc != localFile.LastWriteTimeUtc) {
                    remoteObject.UpdateLastWriteTimeUtc(localFile.LastWriteTimeUtc);
                }
            }

            bool isContentChanged = localFile is IFileInfo ? (localFile as IFileInfo).IsContentChangedTo(mappedObject) : false;

            mappedObject.ParentId = targetId;
            mappedObject.LastChangeToken = remoteObject.ChangeToken;
            mappedObject.LastRemoteWriteTimeUtc = remoteObject.LastModificationDate;
            mappedObject.Name = remoteObject.Name;
            this.Storage.SaveMappedObject(mappedObject);
            if (isContentChanged) {
                throw new ArgumentException("Local file content is also changed => force crawl sync.");
            }
        }
    }
}
