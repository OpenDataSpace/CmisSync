
namespace CmisSync.Lib.Consumer {
    using System;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.FileSystem;

    using log4net;

    public class RepositoryRootDeletedDetection : SyncEventHandler {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RepositoryRootDeletedDetection));

        private readonly IDirectoryInfo path;
        private readonly string absolutePath;
        private bool isRootFolderAvailable = true;

        public event EventHandler<RootExistsEventArgs> RepoRootDeleted;

        public RepositoryRootDeletedDetection(IDirectoryInfo localRootPath) {
            this.path = localRootPath;
            this.absolutePath = this.path.FullName;
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
            if (e is ConfigChangedEvent) {
                return false;
            }

            if (!this.isRootFolderAvailable || e is FSEvent || e is AbstractFolderEvent) {
                var rootFolderWasAvailable = this.isRootFolderAvailable;
                var rootFolderExists = this.IsRootFolderAvailable();
                if (!rootFolderExists) {
                    Logger.Fatal(string.Format("Local root folder \"{0}\" is missing: All events will be ignored until the root folder is back again.", this.absolutePath));
                } else if (!rootFolderWasAvailable) {
                    Logger.Info(string.Format("Local root folder \"{0}\" is available again", this.absolutePath));
                }

                if (rootFolderExists != rootFolderWasAvailable) {
                    var handler = this.RepoRootDeleted;
                    if (handler != null) {
                        handler(this, new RootExistsEventArgs(rootFolderExists));
                    }
                }

                return !rootFolderExists;
            } else {
                return false;
            }
        }

        public class RootExistsEventArgs : EventArgs {
            public RootExistsEventArgs(bool exits) {
                this.RootExists = exits;
            }

            public bool RootExists { get; private set; }
        }

        private bool IsRootFolderAvailable() {
            this.path.Refresh();
            if (!this.path.Exists || this.path.FullName != this.absolutePath) {
                this.isRootFolderAvailable = false;
            } else {
                this.isRootFolderAvailable = true;
            }

            return this.isRootFolderAvailable;
        }
    }
}