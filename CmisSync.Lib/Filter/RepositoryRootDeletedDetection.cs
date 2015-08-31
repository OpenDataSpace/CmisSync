//-----------------------------------------------------------------------
// <copyright file="RepositoryRootDeletedDetection.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Filter {
    using System;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.FileSystem;

    using log4net;

    /// <summary>
    /// Repository root deleted detection.
    /// </summary>
    public class RepositoryRootDeletedDetection : SyncEventHandler {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RepositoryRootDeletedDetection));

        private readonly IDirectoryInfo path;
        private readonly string absolutePath;
        private bool isRootFolderAvailable = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Filter.RepositoryRootDeletedDetection"/> class.
        /// </summary>
        /// <param name="localRootPath">Local root path.</param>
        public RepositoryRootDeletedDetection(IDirectoryInfo localRootPath) {
            if (localRootPath == null) {
                throw new ArgumentNullException("localRootPath");
            }

            this.path = localRootPath;
            this.absolutePath = this.path.FullName;
            this.isRootFolderAvailable = this.IsRootFolderAvailable();
        }

        /// <summary>
        /// Occurs when repo root is deleted.
        /// </summary>
        public event EventHandler<RootExistsEventArgs> RepoRootDeleted;

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

        private bool IsRootFolderAvailable() {
            this.path.Refresh();
            if (!this.path.Exists || this.path.FullName != this.absolutePath) {
                this.isRootFolderAvailable = false;
            } else {
                this.isRootFolderAvailable = true;
            }

            return this.isRootFolderAvailable;
        }

        /// <summary>
        /// Root exists event arguments.
        /// </summary>
        public class RootExistsEventArgs : EventArgs {
            /// <summary>
            /// Initializes a new instance of the
            /// <see cref="CmisSync.Lib.Filter.RepositoryRootDeletedDetection+RootExistsEventArgs"/> class.
            /// </summary>
            /// <param name="exists">The root exists if set to <c>true</c>.</param>
            public RootExistsEventArgs(bool exists) {
                this.RootExists = exists;
            }

            /// <summary>
            /// Gets a value indicating whether root exists.
            /// </summary>
            /// <value><c>true</c> if root exists; otherwise, <c>false</c>.</value>
            public bool RootExists { get; private set; }
        }
    }
}