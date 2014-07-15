//-----------------------------------------------------------------------
// <copyright file="LocalObjectFetcher.cs" company="GRAU DATA AG">
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
//-------------------
namespace CmisSync.Lib.Sync.Strategy
{
    using System;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;

    using log4net;

    /// <summary>
    /// Local object fetcher. I generates IFileSystemInfo if none found in FileEvent or FolderEvent
    /// </summary>
    /// <exception cref='ArgumentNullException'>
    /// Is thrown when an argument passed to a method is invalid because it is <see langword="null" /> .
    /// </exception>
    public class LocalObjectFetcher : SyncEventHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LocalObjectFetcher));

        private IFileSystemInfoFactory fsFactory;

        private IPathMatcher matcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Sync.Strategy.LocalObjectFetcher"/> class.
        /// </summary>
        /// <param name='matcher'>
        /// Matcher from IMetaDataStorage
        /// </param>
        /// <param name='fsFactory'>
        /// Fs factory, should be null unless in Unit Tests
        /// </param>
        /// <exception cref='ArgumentNullException'>
        /// Is thrown when an argument passed to a method is invalid because it is <see langword="null" /> .
        /// </exception>
        public LocalObjectFetcher(IPathMatcher matcher, IFileSystemInfoFactory fsFactory = null) {
            if (matcher == null) {
                throw new ArgumentNullException("matcher can not be null");
            }

            this.matcher = matcher;
            if(fsFactory == null) {
                this.fsFactory = new FileSystemInfoFactory();
            } else {
                this.fsFactory = fsFactory;
            }
        }

        /// <summary>
        /// Handle the specified e if FolderEvent of FileEvent.
        /// </summary>
        /// <param name='e'>
        /// Any ISyncEvent
        /// </param>
        /// <returns>always false</returns>
        public override bool Handle(ISyncEvent e) {
            if (e is FolderEvent) {
                var folderEvent = e as FolderEvent;
                if(folderEvent.LocalFolder != null) {
                    return false;
                }

                if (!this.matcher.CanCreateLocalPath(folderEvent.RemoteFolder.Path)) {
                    Logger.Debug("Dropping FolderEvent for not accessable path: " + folderEvent.RemoteFolder.Path);
                    return true;
                }

                Logger.Debug("Fetching local object for " + folderEvent);
                string localPath = this.matcher.CreateLocalPath(folderEvent.RemoteFolder.Path);
                folderEvent.LocalFolder = this.fsFactory.CreateDirectoryInfo(localPath);
            }

            if (e is FileEvent) {
                var fileEvent = e as FileEvent;
                if (fileEvent.LocalFile != null) {
                    return false;
                }

                if (!this.matcher.CanCreateLocalPath(fileEvent.RemoteFile.Paths[0])) {
                    Logger.Debug("Dropping FileEvent for not accessable path: " + fileEvent.RemoteFile.Paths[0]);
                    return true;
                }

                Logger.Debug("Fetching local object for " + fileEvent);
                string localPath = this.matcher.CreateLocalPath(fileEvent.RemoteFile.Paths[0]);
                fileEvent.LocalFile = this.fsFactory.CreateFileInfo(localPath);
            }

            return false;
        }
    }
}