//-----------------------------------------------------------------------
// <copyright file="Crawler.cs" company="GRAU DATA AG">
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
namespace CmisSync.Lib.Sync.Strategy
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;

    using DotCMIS.Client;

    using log4net;

    /// <summary>
    /// Crawler Strategy which crawls local and remote directories for finding differences between them.
    /// </summary>
    public class Crawler : ReportingSyncEventHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Crawler));

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Sync.Strategy.Crawler"/> class.
        /// </summary>
        /// <param name='queue'>
        /// Queue to report events to.
        /// </param>
        /// <param name='remoteFolder'>
        /// Remote folder, which is the root of the crawl strategy.
        /// </param>
        /// <param name='localFolder'>
        /// Local folder, which is the root of the crawl strategy.
        /// </param>
        /// <param name='storage'>
        /// Meta Data Storage. Null is forbidden and produces ArgumentNullException
        /// </param>
        /// <param name='fsFactory'>
        /// Factory for everyThing FileSystem related. Null leaves the default which is fine.
        /// </param>
        public Crawler(ISyncEventQueue queue, IFolder remoteFolder, IDirectoryInfo localFolder, IMetaDataStorage storage, IFileSystemInfoFactory fsFactory = null) : base(queue)
        {
            if (localFolder == null) {
                throw new ArgumentNullException("Given local folder is null");
            }

            if (remoteFolder == null) {
                throw new ArgumentNullException("Given remote folder is null");
            }

            if (storage == null) {
                throw new ArgumentNullException("Given storage is null");
            }

            this.RemoteFolder = remoteFolder;
            this.LocalFolder = localFolder;
            this.Storage = storage;

            if (fsFactory == null) {
                this.FsFactory = new FileSystemInfoFactory();
            } else {
                this.FsFactory = fsFactory;
            }
        }

        /// <summary>
        /// Gets the remote folder.
        /// </summary>
        /// <value>The remote folder.</value>
        public IFolder RemoteFolder { get; private set; }

        /// <summary>
        /// Gets the local folder.
        /// </summary>
        /// <value>The local folder.</value>
        public IDirectoryInfo LocalFolder { get; private set; }

        /// <summary>
        /// Gets the file system info factory.
        /// </summary>
        /// <value>The fs factory.</value>
        public IFileSystemInfoFactory FsFactory { get; private set; }

        /// <summary>
        /// Gets the meta data storage.
        /// </summary>
        /// <value>The storage.</value>
        public IMetaDataStorage Storage { get; private set; }

        /// <summary>
        /// Handles the specified e.
        /// </summary>
        /// <param name='e'>
        /// If set to <c>true</c> e.
        /// </param>
        /// <returns>true if event handled</returns>
        public override bool Handle(ISyncEvent e)
        {
            if(e is CrawlRequestEvent) {
                var request = e as CrawlRequestEvent;
                Logger.Debug("Starting CrawlSync upon " + request);
                this.CrawlSync(request.RemoteFolder, request.LocalFolder);
                return true;
            }

            if(e is StartNextSyncEvent) {
                Logger.Debug("Starting CrawlSync upon " + e);
                this.CrawlSync(this.RemoteFolder, this.LocalFolder);
                Queue.AddEvent(new FullSyncCompletedEvent(e as StartNextSyncEvent));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Synchronize by checking all folders/files one-by-one.
        /// This strategy is used if the CMIS server does not support the ChangeLog feature or as fallback if other methods failed.
        /// </summary>
        /// <param name="remoteFolder">
        /// CmisObject of the remote Folder
        /// </param>
        /// <param name="localFolder">
        /// IDirectoryInfo of the local Folder
        /// </param>
        private void CrawlSync(IFolder remoteFolder, IDirectoryInfo localFolder)
        {
            // Sets of local files/folders.
            ISet<string> localFileNames = new HashSet<string>();
            ISet<string> localDirNames = new HashSet<string>();

            // Collect all local folder and file names existing in local folder
            foreach(IDirectoryInfo subdir in localFolder.GetDirectories())
            {
                localDirNames.Add(subdir.Name);
            }

            foreach(IFileInfo file in localFolder.GetFiles())
            {
                localFileNames.Add(file.Name);
            }

            if (remoteFolder == null) {
                Logger.Debug(string.Format("Remote Folder is null => skipping crawl sync on local folder \"{0}\"", localFolder.FullName));
                return;
            }

            // Collect all child objects of the remote folder and figure out differences
            IItemEnumerable<ICmisObject> remoteChildren = remoteFolder.GetChildren();
            foreach (ICmisObject cmisObject in remoteChildren) {
                if (cmisObject is IFolder) {
                    IFolder folder = cmisObject as IFolder;
                    if(localDirNames.Contains(folder.Name)) {
                        // Both sides do have got the same folder name
                        // Synchronize metadata if different
                        Queue.AddEvent(new FolderEvent(
                            localFolder: this.FsFactory.CreateDirectoryInfo(Path.Combine(localFolder.FullName, folder.Name)),
                            remoteFolder: folder,
                            src: this));

                        // Remove handled folder from set to get only the local only folders back from set if done
                        localDirNames.Remove(folder.Name);
                    } else {
                        // Remote folder detected, which is not available locally
                        // Figure out, what to do with it
                        Queue.AddEvent(new FolderEvent(
                            remoteFolder: folder,
                            src: this) {
                            Remote = MetaDataChangeType.CREATED });
                    }

                    // Recursive crawl the content of the folder
                    Queue.AddEvent(new CrawlRequestEvent(
                        localFolder: this.FsFactory.CreateDirectoryInfo(Path.Combine(localFolder.FullName, folder.Name)),
                        remoteFolder: folder));
                } else if(cmisObject is IDocument) {
                    IDocument doc = cmisObject as IDocument;
                    var fileEvent = new FileEvent(
                        localFile: this.FsFactory.CreateFileInfo(Path.Combine(localFolder.FullName, doc.Name)),
                        localParentDirectory: localFolder,
                        remoteFile: doc);
                    if(localFileNames.Contains(doc.Name)) {
                        // Both sides do have got the file, synchronize them if different
                        Queue.AddEvent(fileEvent);

                        // Remove handled file from set
                        localFileNames.Remove(doc.Name);
                    } else {
                        // Only remote has got a file, figure out what to do
                        fileEvent.Remote = MetaDataChangeType.CREATED;

                        if(doc.ContentStreamId != null) {
                            fileEvent.RemoteContent = ContentChangeType.CREATED;
                        }

                        Queue.AddEvent(fileEvent);
                    }
                }
            }

            // Only local folders are available, inform synchronizer about them
            foreach(string folder in localDirNames) {
                Queue.AddEvent(new FolderEvent(
                    localFolder: this.FsFactory.CreateDirectoryInfo(Path.Combine(localFolder.FullName, folder)),
                    src: this) { Local = MetaDataChangeType.CREATED });
                Queue.AddEvent(new CrawlRequestEvent(localFolder: this.FsFactory.CreateDirectoryInfo(Path.Combine(localFolder.FullName, folder)), remoteFolder: null));
            }

            // Only local files are available, inform synchronizer about them
            foreach(string file in localFileNames) {
                Queue.AddEvent(new FileEvent(
                    localFile: this.FsFactory.CreateFileInfo(Path.Combine(localFolder.FullName, file)),
                    localParentDirectory: localFolder)
                        { Local = MetaDataChangeType.CREATED,
                        LocalContent = ContentChangeType.CREATED });
            }
        }
    }
}
