//-----------------------------------------------------------------------
// <copyright file="CrawlRequestEvent.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    /// <summary>
    /// Crawl request event.
    /// </summary>
    public class CrawlRequestEvent : ISyncEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.CrawlRequestEvent"/> class.
        /// </summary>
        /// <param name='localFolder'>
        /// Local folder.
        /// </param>
        /// <param name='remoteFolder'>
        /// Remote folder.
        /// </param>
        public CrawlRequestEvent(IDirectoryInfo localFolder, IFolder remoteFolder)
        {
            if (localFolder == null)
            {
                throw new ArgumentNullException("Given path is null");
            }

            this.RemoteFolder = remoteFolder;
            this.LocalFolder = localFolder;
        }

        /// <summary>
        /// Gets the remote folder.
        /// </summary>
        /// <value>
        /// The remote folder.
        /// </value>
        public IFolder RemoteFolder { get; set; }

        /// <summary>
        /// Gets the local folder.
        /// </summary>
        /// <value>
        /// The local folder.
        /// </value>
        public IDirectoryInfo LocalFolder { get; private set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.CrawlRequestEvent"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.CrawlRequestEvent"/>.</returns>
        public override string ToString()
        {
            return string.Format(
                "[CrawlRequestEvent: RemoteFolder={0}, LocalFolder={1}]",
                this.RemoteFolder == null ? string.Empty : this.RemoteFolder.Name,
                this.LocalFolder.Name);
        }
    }
}
