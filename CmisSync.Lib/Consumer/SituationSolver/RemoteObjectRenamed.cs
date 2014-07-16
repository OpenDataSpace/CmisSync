//-----------------------------------------------------------------------
// <copyright file="RemoteObjectRenamed.cs" company="GRAU DATA AG">
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

    using log4net;

    /// <summary>
    /// Remote object has been renamed. => Rename the corresponding local object.
    /// </summary>
    public class RemoteObjectRenamed : ISolver
    {
        private static readonly ILog OperationsLogger = LogManager.GetLogger("OperationsLogger");

        /// <summary>
        /// Renames the specified localFile to the name of the given remoteId object by using the storage, localFile and remoteId.
        /// </summary>
        /// <param name="session">Cmis session instance. Is not needed to solve this specific situation.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="localFile">Local file or folder. It is the source file/folder reference, which should be renamed.</param>
        /// <param name="remoteId">Remote identifier. Should be an instance of IFolder or IDocument.</param>
        public virtual void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFile, IObjectId remoteId)
        {
            IMappedObject obj = storage.GetObjectByRemoteId(remoteId.Id);
            if(remoteId is IFolder)
            {
                // Rename local folder
                IFolder remoteFolder = remoteId as IFolder;
                IDirectoryInfo dirInfo = localFile as IDirectoryInfo;
                string oldPath = dirInfo.FullName;
                dirInfo.MoveTo(Path.Combine(dirInfo.Parent.FullName, remoteFolder.Name));
                if (remoteFolder.LastModificationDate != null) {
                    dirInfo.LastWriteTimeUtc = (DateTime)remoteFolder.LastModificationDate;
                }

                obj.Name = remoteFolder.Name;
                obj.LastChangeToken = remoteFolder.ChangeToken;
                obj.LastRemoteWriteTimeUtc = remoteFolder.LastModificationDate;
                obj.LastLocalWriteTimeUtc = dirInfo.LastWriteTimeUtc;
                storage.SaveMappedObject(obj);
                OperationsLogger.Info(string.Format("Renamed local folder {0} to {1}", oldPath, remoteFolder.Name));
            }
            else if(remoteId is IDocument)
            {
                // Rename local file
                IDocument remoteDocument = remoteId as IDocument;
                IFileInfo fileInfo = localFile as IFileInfo;
                string oldPath = fileInfo.FullName;
                fileInfo.MoveTo(Path.Combine(fileInfo.Directory.FullName, remoteDocument.Name));
                if (remoteDocument.LastModificationDate != null) {
                    fileInfo.LastWriteTimeUtc = (DateTime)remoteDocument.LastModificationDate;
                }

                obj.Name = remoteDocument.Name;
                obj.LastChangeToken = remoteDocument.ChangeToken;
                obj.LastRemoteWriteTimeUtc = remoteDocument.LastModificationDate;
                obj.LastLocalWriteTimeUtc = fileInfo.LastWriteTimeUtc;
                storage.SaveMappedObject(obj);
                OperationsLogger.Info(string.Format("Renamed local file {0} to {1}", oldPath, remoteDocument.Name));
            } else {
                throw new ArgumentException("Given remote Id is not an IFolder nor an IDocument instance");
            }
        }
    }
}