//-----------------------------------------------------------------------
// <copyright file="FolderEvent.cs" company="GRAU DATA AG">
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
    /// Folder event. Should be added to the Queue if anything on a folder could have been changed.
    /// </summary>
    public class FolderEvent : AbstractFolderEvent, IFilterableNameEvent, IFilterablePathEvent, IFilterableRemoteObjectEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.FolderEvent"/> class.
        /// </summary>
        /// <param name="localFolder">Local folder.</param>
        /// <param name="remoteFolder">Remote folder.</param>
        /// <param name="src">Event creator.</param>
        public FolderEvent(IDirectoryInfo localFolder = null, IFolder remoteFolder = null, object src = null)
        {
            if(localFolder == null && remoteFolder == null)
            {
                throw new ArgumentNullException("One of the given folders must not be null");
            }

            this.LocalFolder = localFolder;
            this.RemoteFolder = remoteFolder;
            this.Source = src;
        }

        /// <summary>
        /// Gets or sets the local folder.
        /// </summary>
        /// <value>The local folder.</value>
        public IDirectoryInfo LocalFolder { get; set; }

        /// <summary>
        /// Gets or sets the remote folder.
        /// </summary>
        /// <value>The remote folder.</value>
        public IFolder RemoteFolder { get; set; }

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>The source.</value>
        public object Source { get; private set; }

        /// <summary>
        /// Gets the folder name.
        /// </summary>
        /// <value>The folder name.</value>
        public string Name {
            get {
                return this.LocalFolder != null ? this.LocalFolder.Name : this.RemoteFolder.Name;
            }
        }

        /// <summary>
        /// Gets the remote path.
        /// </summary>
        /// <value>The path.</value>
        public string Path {
            get {
                return this.RemoteFolder != null ? this.RemoteFolder.Path : null;
            }
        }

        /// <summary>
        /// Gets the remote object.
        /// </summary>
        /// <value>The remote object.</value>
        public ICmisObject RemoteObject {
            get {
                return this.RemoteFolder;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.FolderEvent"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.FolderEvent"/>.</returns>
        public override string ToString()
        {
            return string.Format(
                "[FolderEvent: Local={0} on {2}, Remote={1} on {3} created by {4}]",
                this.Local,
                this.Remote,
                this.LocalFolder != null ? this.LocalFolder.Name : string.Empty,
                this.RemoteFolder != null ? this.RemoteFolder.Name : string.Empty,
                this.Source != null ? this.Source : "null");
        }

        /// <summary>
        /// Determines whether this event contains a directory.
        /// </summary>
        /// <returns><c>true</c></returns>
        public bool IsDirectory() {
            return true;
        }
    }
}
