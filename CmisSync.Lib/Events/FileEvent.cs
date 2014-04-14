//-----------------------------------------------------------------------
// <copyright file="FileDownloadRequest.cs" company="GRAU DATA AG">
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
    /// File event.
    /// </summary>
    public class FileEvent : AbstractFolderEvent
    {
        /// <summary>
        /// Gets or sets the content of the local.
        /// </summary>
        /// <value>
        /// The content of the local.
        /// </value>
        public ContentChangeType LocalContent { get; set; }

        /// <summary>
        /// Gets or sets the content of the remote.
        /// </summary>
        /// <value>
        /// The content of the remote.
        /// </value>
        public ContentChangeType RemoteContent { get; set; }

        /// <summary>
        /// Gets or sets the local file.
        /// </summary>
        /// <value>
        /// The local file.
        /// </value>
        public IFileInfo LocalFile { get; protected set; }

        /// <summary>
        /// Gets or sets the local parent directory.
        /// </summary>
        /// <value>
        /// The local parent directory.
        /// </value>
        public IDirectoryInfo LocalParentDirectory { get; protected set; }

        /// <summary>
        /// Gets or sets the remote file.
        /// </summary>
        /// <value>
        /// The remote file.
        /// </value>
        public IDocument RemoteFile { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.FileEvent"/> class.
        /// </summary>
        /// <param name='localFile'>
        /// Local file.
        /// </param>
        /// <param name='localParentDirectory'>
        /// Local parent directory.
        /// </param>
        /// <param name='remoteFile'>
        /// Remote file.
        /// </param>
        public FileEvent(IFileInfo localFile = null, IDirectoryInfo localParentDirectory = null, IDocument remoteFile = null)
        {
            if (localFile == null && remoteFile == null)
            {
                throw new ArgumentNullException("Given local or remote file must not be null");
            }

            this.LocalFile = localFile;
            this.LocalParentDirectory = localParentDirectory;
            this.RemoteFile = remoteFile;
            this.LocalContent = ContentChangeType.NONE;
            this.RemoteContent = ContentChangeType.NONE;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.FileEvent"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.FileEvent"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("[FileEvent: Local={0}, LocalContent={1}, Remote={2}, RemoteContent={3}]",
                                  this.Local,
                                  this.LocalContent,
                                  this.Remote,
                                  this.RemoteContent);
        }
    }

    /// <summary>
    /// File moved event.
    /// </summary>
    public class FileMovedEvent : FileEvent
    {

        public IFileInfo OldLocalFile{ get; protected set; }

        public IDirectoryInfo OldParentFolder { get; protected set; }

        public string OldRemoteFilePath { get; protected set; }

        public FileMovedEvent(
            IFileInfo oldLocalFile = null,
            IFileInfo newLocalFile = null,
            IDirectoryInfo oldParentFolder = null,
            IDirectoryInfo newParentFolder = null,
            string oldRemoteFilePath = null,
            IDocument newRemoteFile = null
        ) : base(newLocalFile, newParentFolder, newRemoteFile)
        {
            Local = MetaDataChangeType.MOVED;
            OldLocalFile = oldLocalFile;
            OldParentFolder = oldParentFolder;
            OldRemoteFilePath = oldRemoteFilePath;
        }
    }
}
