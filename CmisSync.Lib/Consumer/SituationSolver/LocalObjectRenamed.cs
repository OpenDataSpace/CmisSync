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

namespace CmisSync.Lib.Consumer.SituationSolver {
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Exceptions;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    /// <summary>
    /// Local object has been renamed. => Rename the corresponding object on the server.
    /// </summary>
    public class LocalObjectRenamed : AbstractEnhancedSolver {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Consumer.SituationSolver.LocalObjectRenamed"/> class.
        /// </summary>
        /// <param name="session">Cmis session.</param>
        /// <param name="storage">Meta data storage.</param>
        public LocalObjectRenamed(
            ISession session,
            IMetaDataStorage storage) : base(session, storage) {
        }

        /// <summary>
        /// Solve the specified situation by using localFile and remote object.
        /// </summary>
        /// <param name="localFileSystemInfo">Local file.</param>
        /// <param name="remoteId">Remote identifier or object.</param>
        /// <param name="localContent">Hint if the local content has been changed.</param>
        /// <param name="remoteContent">Information if the remote content has been changed.</param>
        public override void Solve(
            IFileSystemInfo localFileSystemInfo,
            IObjectId remoteId,
            ContentChangeType localContent = ContentChangeType.NONE,
            ContentChangeType remoteContent = ContentChangeType.NONE)
        {
            if (remoteId == null) {
                throw new ArgumentNullException("remoteId");
            }

            if (localFileSystemInfo == null) {
                throw new ArgumentNullException("localFileSystemInfo");
            }

            var obj = this.Storage.GetObjectByRemoteId(remoteId.Id);

            // Rename remote object
            var cmisObject = remoteId as ICmisObject;
            if (cmisObject != null) {
                if (cmisObject.ChangeToken != obj.LastChangeToken) {
                    throw new ArgumentException("Last changetoken is invalid => force crawl sync");
                }

                string oldName = cmisObject.Name;
                string newName = localFileSystemInfo.Name;
                string newFullName = localFileSystemInfo.FullName;
                try {
                    cmisObject.Rename(newName, true);
                } catch (CmisNameConstraintViolationException e) {
                    if (!Utils.IsValidISO885915(newName)) {
                        OperationsLogger.Warn(string.Format("The server denies the renaming of {2} from {0} to {1}, perhaps because the new name contains UTF-8 characters", oldName, newName, newFullName));
                        throw new InteractionNeededException(string.Format("Server denied renaming of {0}", oldName), e) {
                            Title = string.Format("Server denied renaming of {0}", oldName),
                            Description = string.Format("The server denies the renaming of {2} from {0} to {1}, perhaps because the new name contains UTF-8 characters", oldName, newName, newFullName)
                        };
                    } else {
                        try {
                            string wishedRemotePath = this.Storage.Matcher.CreateRemotePath(newFullName);
                            var conflictingRemoteObject = this.Session.GetObjectByPath(wishedRemotePath) as IFileableCmisObject;
                            if (conflictingRemoteObject != null && conflictingRemoteObject.AreAllChildrenIgnored()) {
                                OperationsLogger.Warn(string.Format("The server denies the renaming of {2} from {0} to {1}, because there is an ignored file/folder with this name already, trying to create conflict file/folder name", oldName, newName, newFullName), e);
                                if (localFileSystemInfo is IDirectoryInfo) {
                                    (localFileSystemInfo as IDirectoryInfo).MoveTo(Path.Combine((localFileSystemInfo as IDirectoryInfo).Parent.FullName, newName + "_Conflict"));
                                } else if (localFileSystemInfo is IFileInfo) {
                                    (localFileSystemInfo as IFileInfo).MoveTo(Path.Combine((localFileSystemInfo as IFileInfo).Directory.FullName, newName + "_Conflict"));
                                }

                                return;
                            }
                        } catch (CmisObjectNotFoundException) {
                        }

                        OperationsLogger.Warn(string.Format("The server denies the renaming of {2} from {0} to {1}", oldName, newName, newFullName), e);
                    }

                    throw;
                } catch (CmisPermissionDeniedException) {
                    OperationsLogger.Warn(string.Format("Unable to renamed remote object from {0} to {1}: Permission Denied", oldName, newName));
                    return;
                }

                OperationsLogger.Info(string.Format("Renamed remote object {0} from {1} to {2}", remoteId.Id, oldName, newName));
            } else {
                throw new ArgumentException("Given remoteId type is unknown: " + remoteId.GetType().Name);
            }

            bool isContentChanged = localFileSystemInfo is IFileInfo ? (localFileSystemInfo as IFileInfo).IsContentChangedTo(obj) : false;

            obj.Name = cmisObject.Name;
            obj.LastRemoteWriteTimeUtc = cmisObject.LastModificationDate;
            obj.LastLocalWriteTimeUtc = isContentChanged ? obj.LastLocalWriteTimeUtc : localFileSystemInfo.LastWriteTimeUtc;
            obj.LastChangeToken = cmisObject.ChangeToken;
            obj.Ignored = cmisObject.AreAllChildrenIgnored();
            this.Storage.SaveMappedObject(obj);
            if (isContentChanged) {
                throw new ArgumentException("Local file content is also changed => force crawl sync.");
            }
        }
    }
}