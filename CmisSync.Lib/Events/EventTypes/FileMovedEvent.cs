//-----------------------------------------------------------------------
// <copyright file="FileMovedEvent.cs" company="GRAU DATA AG">
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
    /// File moved event.
    /// </summary>
    public class FileMovedEvent : FileEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.FileMovedEvent"/> class.
        /// </summary>
        /// <param name="oldLocalFile">Old local file.</param>
        /// <param name="newLocalFile">New local file.</param>
        /// <param name="oldParentFolder">Old parent folder.</param>
        /// <param name="newParentFolder">New parent folder.</param>
        /// <param name="oldRemoteFilePath">Old remote file path.</param>
        /// <param name="newRemoteFile">New remote file.</param>
        public FileMovedEvent(
            IFileInfo oldLocalFile = null,
            IFileInfo newLocalFile = null,
            IDirectoryInfo oldParentFolder = null,
            IDirectoryInfo newParentFolder = null,
            string oldRemoteFilePath = null,
            IDocument newRemoteFile = null)
            : base(newLocalFile, newParentFolder, newRemoteFile)
        {
            this.Local = MetaDataChangeType.MOVED;
            this.OldLocalFile = oldLocalFile;
            this.OldParentFolder = oldParentFolder;
            this.OldRemoteFilePath = oldRemoteFilePath;
        }

        /// <summary>
        /// Gets or sets the old local file.
        /// </summary>
        /// <value>The old local file.</value>
        public IFileInfo OldLocalFile { get; protected set; }

        /// <summary>
        /// Gets or sets the old parent folder.
        /// </summary>
        /// <value>The old parent folder.</value>
        public IDirectoryInfo OldParentFolder { get; protected set; }

        /// <summary>
        /// Gets or sets the old remote file path.
        /// </summary>
        /// <value>The old remote file path.</value>
        public string OldRemoteFilePath { get; protected set; }
    }
}
