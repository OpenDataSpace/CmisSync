//-----------------------------------------------------------------------
// <copyright file="DecendantsCrawler.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    using DotCMIS.Client;

    using log4net;

    /// <summary>
    /// Decendants crawler.
    /// </summary>
    public class DecendantsCrawler : Crawler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DecendantsCrawler));

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Sync.Strategy.DecendantsCrawler"/> class.
        /// </summary>
        /// <param name="queue">Sync Event Queue.</param>
        /// <param name="remoteFolder">Remote folder.</param>
        /// <param name="localFolder">Local folder.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="fsFactory">File system info factory.</param>
        public DecendantsCrawler(
            ISyncEventQueue queue,
            IFolder remoteFolder,
            IDirectoryInfo localFolder, 
            IMetaDataStorage storage,
            IFileSystemInfoFactory fsFactory = null)
            : base(queue, remoteFolder, localFolder, storage, fsFactory)
        {
        }

        public override bool Handle(ISyncEvent e)
        {
            if(e is StartNextSyncEvent) {
                Logger.Debug("Starting DecendantsCrawlSync upon " + e);
                this.CrawlDescendants();
                this.Queue.AddEvent(new FullSyncCompletedEvent(e as StartNextSyncEvent));
                return true;
            }

            return false;
        }

        private void CrawlDescendants() {
            var desc = this.RemoteFolder.GetDescendants(-1);
        }
    }
}