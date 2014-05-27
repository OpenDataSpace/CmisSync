//-----------------------------------------------------------------------
// <copyright file="FileOrFolderEventFactory.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Events
{
    using System;

    using CmisSync.Lib.Storage;

    using DotCMIS.Client;

    /// <summary>
    /// File or folder event factory.
    /// </summary>
    public static class FileOrFolderEventFactory
    {
        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <returns>The event.</returns>
        /// <param name="isFile">If set to <c>true</c> is file.</param>
        /// <param name="remoteObject">Remote object.</param>
        /// <param name="localObject">Local object.</param>
        /// <param name="remoteChange">Remote change.</param>
        /// <param name="localChange">Local change.</param>
        /// <param name="oldRemotePath">Old remote path.</param>
        /// <param name="oldLocalObject">Old local object.</param>
        /// <param name="src">Source of the creation.</param>
        public static AbstractFolderEvent CreateEvent(
            bool isFile,
            IFileableCmisObject remoteObject = null,
            IFileSystemInfo localObject = null,
            MetaDataChangeType remoteChange = MetaDataChangeType.NONE,
            MetaDataChangeType localChange = MetaDataChangeType.NONE,
            string oldRemotePath = null,
            IFileSystemInfo oldLocalObject = null,
            object src = null) {
            if (localChange != MetaDataChangeType.MOVED &&
                remoteChange != MetaDataChangeType.MOVED) {
                if (isFile) {
                    return new FileEvent(
                        localObject as IFileInfo,
                        null, 
                        remoteObject as IDocument) {
                        Local = localChange,
                        Remote = remoteChange
                    };
                } else {
                    return new FolderEvent(
                        localObject as IDirectoryInfo,
                        remoteObject as IFolder,
                        src) {
                        Local = localChange,
                        Remote = remoteChange
                    };
                }
            } else {
                if (isFile) {
                    return new FileMovedEvent(
                        oldLocalObject as IFileInfo,
                        localObject as IFileInfo,
                        null,
                        null,
                        oldRemotePath,
                        remoteObject as IDocument);
                } else {
                    return new FolderMovedEvent(
                        oldLocalObject as IDirectoryInfo,
                        localObject as IDirectoryInfo,
                        oldRemotePath,
                        remoteObject as IFolder);
                }
            }
        }
    }
}