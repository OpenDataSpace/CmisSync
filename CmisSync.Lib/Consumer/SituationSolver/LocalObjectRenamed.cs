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

namespace CmisSync.Lib.Consumer.SituationSolver
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    /// <summary>
    /// Local object has been renamed. => Rename the corresponding object on the server.
    /// </summary>
    public class LocalObjectRenamed : AbstractEnhancedSolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Consumer.SituationSolver.LocalObjectRenamed"/> class.
        /// </summary>
        /// <param name="session">Cmis session.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="serverCanModifyCreationAndModificationDate">If set to <c>true</c> server can modify creation and modification date.</param>
        public LocalObjectRenamed(
            ISession session,
            IMetaDataStorage storage) : base(session, storage) {
        }

        /// <summary>
        /// Solve the specified situation by using localFile and remote object.
        /// </summary>
        /// <param name="localFile">Local file.</param>
        /// <param name="remoteId">Remote identifier or object.</param>
        /// <param name="localContent">Hint if the local content has been changed.</param>
        /// <param name="remoteContent">Information if the remote content has been changed.</param>
        public override void Solve(
            IFileSystemInfo localFile,
            IObjectId remoteId,
            ContentChangeType localContent = ContentChangeType.NONE,
            ContentChangeType remoteContent = ContentChangeType.NONE)
        {
            var obj = this.Storage.GetObjectByRemoteId(remoteId.Id);

            // Rename remote object
            if (remoteId is ICmisObject) {
                if ((remoteId as ICmisObject).ChangeToken != obj.LastChangeToken) {
                    throw new ArgumentException("Last changetoken is invalid => force crawl sync");
                }

                string oldName = (remoteId as ICmisObject).Name;
                try {
                    (remoteId as ICmisObject).Rename(localFile.Name, true);
                } catch (CmisConstraintException e) {
                    if (!Utils.IsValidISO885915(localFile.Name)) {
                        OperationsLogger.Warn(string.Format("The server denies the renaming of {2} from {0} to {1}, perhaps because the new name contains UTF-8 characters", oldName, localFile.Name, localFile.FullName));
                        throw new InteractionNeededException(string.Format("Server denied renaming of {0}", oldName), e) {
                            Title = string.Format("Server denied renaming of {0}", oldName),
                            Description = string.Format("The server denies the renaming of {2} from {0} to {1}, perhaps because the new name contains UTF-8 characters", oldName, localFile.Name, localFile.FullName)
                        };
                    } else {
                        OperationsLogger.Warn(string.Format("The server denies the renaming of {2} from {0} to {1}", oldName, localFile.Name, localFile.FullName), e);
                    }

                    throw;
                } catch (CmisPermissionDeniedException) {
                    OperationsLogger.Warn(string.Format("Unable to renamed remote object from {0} to {1}: Permission Denied", oldName, localFile.Name));
                    return;
                }

                OperationsLogger.Info(string.Format("Renamed remote object {0} from {1} to {2}", remoteId.Id, oldName, localFile.Name));
            } else {
                throw new ArgumentException("Given remoteId type is unknown: " + remoteId.GetType().Name);
            }

            bool isContentChanged = localFile is IFileInfo ? (localFile as IFileInfo).IsContentChangedTo(obj) : false;

            obj.Name = (remoteId as ICmisObject).Name;
            obj.LastRemoteWriteTimeUtc = (remoteId as ICmisObject).LastModificationDate;
            obj.LastLocalWriteTimeUtc = isContentChanged ? obj.LastLocalWriteTimeUtc : localFile.LastWriteTimeUtc;
            obj.LastChangeToken = (remoteId as ICmisObject).ChangeToken;
            this.Storage.SaveMappedObject(obj);
            if (isContentChanged) {
                throw new ArgumentException("Local file content is also changed => force crawl sync.");
            }
        }
    }
}