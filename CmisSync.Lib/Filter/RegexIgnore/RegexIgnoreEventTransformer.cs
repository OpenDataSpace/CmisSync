using DotCMIS.Client;
using CmisSync.Lib.PathMatcher;
using CmisSync.Lib.Storage.Database;


namespace CmisSync.Lib.Filter.RegexIgnore {
    using System;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;

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
                    this.queue.AddEvent(new FSEvent(WatcherChangeTypes.Created, movedEvent.LocalPath, movedEvent.IsDirectory));
                    return true;
                } else if (IsInsideIgnoredPath(movedEvent.LocalPath) && !IsInsideIgnoredPath(movedEvent.OldPath)) {
                    this.queue.AddEvent(new FSEvent(WatcherChangeTypes.Deleted, movedEvent.OldPath, movedEvent.IsDirectory));
                    return true;
                }
            }

            var contentChangeEvent = e as ContentChangeEvent;
            if (contentChangeEvent != null) {
                if (contentChangeEvent.Type == ChangeType.Updated) {
                    var cmisObject = contentChangeEvent.CmisObject as IFileableCmisObject;
                    var objectId = cmisObject.Id;
                    var storedObject = storage.GetObjectByRemoteId(objectId);
                    if (storedObject == null) {
                        queue.AddEvent(new ContentChangeEvent(ChangeType.Created, objectId));
                        return true;
                    } else {
                        var localPath = this.matcher.CreateLocalPath(cmisObject.Paths[0]);
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
            return this.filter.CheckFolderPath(path, out reason);
        }
    }
}