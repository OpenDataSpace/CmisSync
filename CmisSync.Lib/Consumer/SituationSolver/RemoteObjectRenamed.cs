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

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    /// <summary>
    /// Remote object has been renamed. => Rename the corresponding local object.
    /// </summary>
    public class RemoteObjectRenamed : AbstractEnhancedSolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Consumer.SituationSolver.RemoteObjectRenamed"/> class.
        /// </summary>
        /// <param name="session">Cmis session.</param>
        /// <param name="storage">Meta data storage.</param>
        public RemoteObjectRenamed(ISession session, IMetaDataStorage storage) : base(session, storage) {
        }

        /// <summary>
        /// Renames the specified localFile to the name of the given remoteId object by using the storage, localFile and remoteId.
        /// </summary>
        /// <param name="localFile">Local file or folder. It is the source file/folder reference, which should be renamed.</param>
        /// <param name="remoteId">Remote identifier. Should be an instance of IFolder or IDocument.</param>
        /// <param name="localContent">Hint if the local content has been changed.</param>
        /// <param name="remoteContent">Information if the remote content has been changed.</param>
        public override void Solve(
            IFileSystemInfo localFile,
            IObjectId remoteId,
            ContentChangeType localContent = ContentChangeType.NONE,
            ContentChangeType remoteContent = ContentChangeType.NONE)
        {
            IMappedObject obj = this.Storage.GetObjectByRemoteId(remoteId.Id);
            if (remoteId is IFolder) {
                // Rename local folder
                IFolder remoteFolder = remoteId as IFolder;
                IDirectoryInfo dirInfo = localFile as IDirectoryInfo;
                string oldPath = dirInfo.FullName;
                try {
                    dirInfo.MoveTo(Path.Combine(dirInfo.Parent.FullName, remoteFolder.Name));
                    obj.Name = remoteFolder.Name;
                } catch (IOException) {
                    if (dirInfo.Name.Equals(remoteFolder.Name, StringComparison.OrdinalIgnoreCase)) {
                        obj.Name = dirInfo.Name;
                    } else {
                        throw;
                    }
                }

                dirInfo.TryToSetReadOnlyStateIfDiffers(from: remoteFolder);
                dirInfo.TryToSetLastWriteTimeUtcIfAvailable(from: remoteFolder);
                obj.LastChangeToken = remoteFolder.ChangeToken;
                obj.LastRemoteWriteTimeUtc = remoteFolder.LastModificationDate;
                obj.LastLocalWriteTimeUtc = dirInfo.LastWriteTimeUtc;
                obj.Ignored = remoteFolder.AreAllChildrenIgnored();
                obj.IsReadOnly = dirInfo.ReadOnly;
                this.Storage.SaveMappedObject(obj);
                OperationsLogger.Info(string.Format("Renamed local folder {0} to {1}", oldPath, remoteFolder.Name));
            } else if(remoteId is IDocument) {
                // Rename local file
                IDocument remoteDocument = remoteId as IDocument;
                IFileInfo fileInfo = localFile as IFileInfo;
                string oldPath = fileInfo.FullName;
                fileInfo.MoveTo(Path.Combine(fileInfo.Directory.FullName, remoteDocument.Name));
                fileInfo.TryToSetReadOnlyStateIfDiffers(from: remoteDocument);
                fileInfo.TryToSetLastWriteTimeUtcIfAvailable(from: remoteDocument);
                obj.Name = remoteDocument.Name;
                obj.LastChangeToken = remoteContent == ContentChangeType.NONE ? remoteDocument.ChangeToken : obj.LastChangeToken;
                obj.LastRemoteWriteTimeUtc = remoteContent == ContentChangeType.NONE ? remoteDocument.LastModificationDate : obj.LastRemoteWriteTimeUtc;
                obj.LastLocalWriteTimeUtc = fileInfo.LastWriteTimeUtc;
                obj.IsReadOnly = fileInfo.ReadOnly;
                this.Storage.SaveMappedObject(obj);
                OperationsLogger.Info(string.Format("Renamed local file {0} to {1}", oldPath, remoteDocument.Name));
                if (remoteContent != ContentChangeType.NONE) {
                    throw new ArgumentException("Remote documents content is also changed => force crawl sync.");
                }
            } else {
                throw new ArgumentException("Given remote Id is not an IFolder nor an IDocument instance");
            }
        }
    }
}