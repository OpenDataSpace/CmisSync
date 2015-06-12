//-----------------------------------------------------------------------
// <copyright file="AbstractNotifyingRepository.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Cmis {
    using System;
    using System.ComponentModel;

    using CmisSync.Lib.Config;

    /// <summary>
    /// Abstract repository class with all notifying components for the front end usage.
    /// </summary>
    public abstract class AbstractNotifyingRepository : INotifyRepositoryPropertyChanged {
        private SyncStatus status = SyncStatus.Disconnected;

        private int changesFound = 0;

        private DateTime? lastFinishedSync;

        protected RepositoryStatus RepoStatusFlags = new RepositoryStatus();

        private RepoInfo repoInfo;

        private string localPath;
        private string name;
        private Uri url;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Cmis.AbstractNotifyingRepository"/> class.
        /// </summary>
        /// <param name="repoInfo">Repo info. Must not be null.</param>
        public AbstractNotifyingRepository(RepoInfo repoInfo) {
            this.Status = this.RepoStatusFlags.Status;
            if (repoInfo == null) {
                throw new ArgumentNullException("repoInfo");
            }

            // Initialize local variable
            this.RepoInfo = repoInfo;
        }

        /// <summary>
        /// Occurs when an exception should be shown to the user.
        /// </summary>
        public event EventHandler<RepositoryExceptionEventArgs> ShowException;

        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the name of the synchronized folder, as found in the CmisSync XML configuration file.
        /// </summary>
        public string Name {
            get {
                return this.name;
            }

            private set {
                if (this.name != value) {
                    this.name = value;
                    this.NotifyPropertyChanged(Utils.NameOf(() => this.Name));
                }
            }
        }

        /// <summary>
        /// Gets the URL of the remote CMIS endpoint.
        /// </summary>
        public Uri RemoteUrl {
            get {
                return this.url;
            }

            private set {
                if (this.url != value) {
                    this.url = value;
                    this.NotifyPropertyChanged(Utils.NameOf(() => this.RemoteUrl));
                }
            }
        }

        /// <summary>
        /// Gets the path of the local synchronized folder.
        /// </summary>
        public string LocalPath {
            get {
                return this.localPath;
            }

            private set {
                if (this.localPath != value) {
                    this.localPath = value;
                    this.NotifyPropertyChanged(Utils.NameOf(() => this.LocalPath));
                }
            }
        }

        /// <summary>
        /// Gets or sets the current status of the synchronization (paused or not).
        /// </summary>
        /// <value>The status.</value>
        public SyncStatus Status {
            get {
                return this.status;
            }

            protected set {
                if (value != this.status) {
                    this.status = value;
                    this.NotifyPropertyChanged(Utils.NameOf(() => this.Status));
                }
            }
        }

        /// <summary>
        /// Gets or sets the last time when a sync was finished without detected changes.
        /// </summary>
        /// <value>The last finished sync.</value>
        public DateTime? LastFinishedSync {
            get {
                return this.lastFinishedSync;
            }

            protected set {
                if (value != this.lastFinishedSync) {
                    this.lastFinishedSync = value;
                    this.NotifyPropertyChanged(Utils.NameOf(() => this.LastFinishedSync));
                }
            }
        }

        /// <summary>
        /// Gets or sets the number of changes which are actually found on queue.
        /// </summary>
        /// <value>The number of changes.</value>
        public int NumberOfChanges {
            get {
                return this.changesFound;
            }

            protected set {
                if (value != this.changesFound) {
                    this.RepoStatusFlags.KnownChanges = value;
                    this.changesFound = value;
                    this.NotifyPropertyChanged(Utils.NameOf(() => this.NumberOfChanges));
                    this.Status = this.RepoStatusFlags.Status;
                }
            }
        }

        /// <summary>
        /// Gets or sets the synchronized folder's information.
        /// </summary>
        protected RepoInfo RepoInfo {
            get {
                return this.repoInfo;
            }

            set {
                this.repoInfo = value;
                this.Name = this.RepoInfo.DisplayName;
                this.LocalPath = this.RepoInfo.LocalPath;
                this.RemoteUrl = this.RepoInfo.Address;
            }
        }

        /// <summary>
        /// Passes the exception to listener.
        /// </summary>
        /// <param name="level">Exception level.</param>
        /// <param name="type">Exception type.</param>
        /// <param name="source">Source exception.</param>
        protected void PassExceptionToListener(ExceptionLevel level, ExceptionType type, Exception source = null) {
            var handler = this.ShowException;
            if (handler != null) {
                handler(this, new RepositoryExceptionEventArgs(level, type, source));
            }
        }

        /// <summary>
        /// This method is called by the Set accessor of each property.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        protected void NotifyPropertyChanged(string propertyName) {
            if (string.IsNullOrEmpty(propertyName)) {
                throw new ArgumentNullException("propertyName");
            }

            var handler = this.PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}