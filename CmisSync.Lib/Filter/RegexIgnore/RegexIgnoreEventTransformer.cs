//-----------------------------------------------------------------------
// <copyright file="RegexIgnoreEventTransformer.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Filter.RegexIgnore {
    using System;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.PathMatcher;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;

    ﻿using DotCMIS.Client;
    using DotCMIS.Enums;

    /// <summary>
    /// Regex ignore event transformer. Transforms move events to create/delete events if the source or target folder is ignored by any regex.
    /// </summary>
    public class RegexIgnoreEventTransformer : SyncEventHandler {
        private IgnoredFolderNameFilter filter;
        private ISyncEventQueue queue;
        private IPathMatcher matcher;
        private IMetaDataStorage storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Filter.RegexIgnore.RegexIgnoreEventTransformer"/> class.
        /// </summary>
        /// <param name="folderNameFilter">Folder name filter.</param>
        /// <param name="queue">Event Queue to put new Events into.</param>
        /// <param name="matcher">Matcher for local and remote paths.</param>
        /// <param name="storage">Meta data storage.</param>
        public RegexIgnoreEventTransformer(
            IgnoredFolderNameFilter folderNameFilter,
            ISyncEventQueue queue,
            IPathMatcher matcher,
            IMetaDataStorage storage)
        {
            if (folderNameFilter == null) {
                throw new ArgumentNullException("folderNameFilter");
            }

            if (queue == null) {
                throw new ArgumentNullException("queue");
            }

            if (matcher == null) {
                throw new ArgumentNullException("matcher");
            }

            if (storage == null) {
                throw new ArgumentNullException("storage");
            }

            this.filter = folderNameFilter;
            this.queue = queue;
            this.matcher = matcher;
            this.storage = storage;
        }

        /// <summary>
        /// Handle FSMovedEvents or ContentChange events and transforms them into correct create or delete.
        /// </summary>
        /// <param name="e">The event to handle.</param>
        /// <returns>true if handled</returns>
        public override bool Handle(ISyncEvent e) {
            var movedEvent = e as FSMovedEvent;
            if (movedEvent != null) {
                if (IsInsideIgnoredPath(movedEvent.OldPath) && !IsInsideIgnoredPath(movedEvent.LocalPath)) {
                    queue.AddEvent(new FSEvent(WatcherChangeTypes.Created, movedEvent.LocalPath, movedEvent.IsDirectory));
                    queue.AddEvent(new StartNextSyncEvent(fullSyncRequested: true));
                    return true;
                } else if (IsInsideIgnoredPath(movedEvent.LocalPath) && !IsInsideIgnoredPath(movedEvent.OldPath)) {
                    queue.AddEvent(new FSEvent(WatcherChangeTypes.Deleted, movedEvent.OldPath, movedEvent.IsDirectory));
                    return true;
                }
            }

            var contentChangeEvent = e as ContentChangeEvent;
            if (contentChangeEvent != null) {
                if (contentChangeEvent.Type == ChangeType.Updated) {
                    var cmisObject = contentChangeEvent.CmisObject as IFileableCmisObject;
                    var objectId = cmisObject.Id;
                    if (storage.GetObjectByRemoteId(objectId) == null) {
                        queue.AddEvent(new ContentChangeEvent(ChangeType.Created, objectId));
                        queue.AddEvent(new StartNextSyncEvent(fullSyncRequested: true));
                        return true;
                    } else {
                        var localPath = matcher.CreateLocalPath(cmisObject.Paths[0]);
                        if (IsInsideIgnoredPath(localPath)) {
                            queue.AddEvent(new ContentChangeEvent(ChangeType.Deleted, objectId));
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool IsInsideIgnoredPath(string path) {
            string reason;
            return filter.CheckFolderPath(path, out reason);
        }
    }
}