
namespace CmisSync.Lib.Consumer {
    using System;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.FileSystem;

    public class RepositoryRootDeletedDetection : SyncEventHandler {
        private readonly IDirectoryInfo path;
        private readonly string absolutePath;
        private readonly Action callback;
        private bool isRootFolderAvailable = true;
        public RepositoryRootDeletedDetection(IDirectoryInfo localRootPath, Action callback) {
            this.path = localRootPath;
            this.absolutePath = this.path.FullName;
            this.callback = callback;
            this.isRootFolderAvailable = this.IsRootFolderAvailable();
        }

        /// <summary>
        /// Priority is CRITICAL
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        public override int Priority {
            get {
                return EventHandlerPriorities.CRITICAL;
            }
        }

        public override bool Handle(ISyncEvent e) {
            if (!this.isRootFolderAvailable || e is FSEvent || e is AbstractFolderEvent) {
                return !this.IsRootFolderAvailable();
            } else {
                return false;
            }
        }

        private bool IsRootFolderAvailable() {
            this.path.Refresh();
            if (!this.path.Exists || this.path.FullName != this.absolutePath) {
                this.isRootFolderAvailable = false;
                this.callback();
            } else {
                this.isRootFolderAvailable = true;
            }

            return this.isRootFolderAvailable;
        }
    }
}