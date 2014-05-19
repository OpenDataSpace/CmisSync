//-----------------------------------------------------------------------
// <copyright file="FolderMovedEvent.cs" company="GRAU DATA AG">
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
    using System.IO;

    using CmisSync.Lib.Storage;

    using DotCMIS.Client;

    /// <summary>
    /// Folder moved event.
    /// </summary>
    public class FolderMovedEvent : FolderEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.FolderMovedEvent"/> class.
        /// </summary>
        /// <param name="oldLocalFolder">Old local folder.</param>
        /// <param name="newLocalFolder">New local folder.</param>
        /// <param name="oldRemoteFolderPath">Old remote folder path.</param>
        /// <param name="newRemoteFolder">New remote folder.</param>
        public FolderMovedEvent(
            IDirectoryInfo oldLocalFolder,
            IDirectoryInfo newLocalFolder,
            string oldRemoteFolderPath,
            IFolder newRemoteFolder) : base(newLocalFolder, newRemoteFolder) {
            this.OldLocalFolder = oldLocalFolder;
            this.OldRemoteFolderPath = oldRemoteFolderPath;
        }

        /// <summary>
        /// Gets the old local folder.
        /// </summary>
        /// <value>The old local folder.</value>
        public IDirectoryInfo OldLocalFolder { get; private set; }

        /// <summary>
        /// Gets the old remote folder path.
        /// </summary>
        /// <value>The old remote folder path.</value>
        public string OldRemoteFolderPath { get; private set; }
    }
}