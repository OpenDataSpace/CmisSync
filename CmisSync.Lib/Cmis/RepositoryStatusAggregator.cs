//-----------------------------------------------------------------------
// <copyright file="RepositoryStatusAggregator.cs" company="GRAU DATA AG">
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
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// Repository status aggregator. All added repositories are used to calculate an aggregated sync status over all repos.
    /// </summary>
    public class RepositoryStatusAggregator : INotifyPropertyChanged {
        private Dictionary<Repository, PropertyChangedEventHandler> repos = new Dictionary<Repository, PropertyChangedEventHandler>();
        private SyncStatus status = SyncStatus.Disconnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Cmis.RepositoryStatusAggregator"/> class.
        /// </summary>
        public RepositoryStatusAggregator() {
        }

        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <value>The status.</value>
        public SyncStatus Status {
            get {
                return this.status;
            }

            private set {
                if (value != this.status) {
                    this.status = value;
                    this.NotifyPropertyChanged(Utils.NameOf(() => this.Status));
                }
            }
        }

        /// <summary>
        /// Add the specified repo.
        /// </summary>
        /// <param name="repo">Repository to listen to.</param>
        public void Add(Repository repo) {
            if (!this.repos.ContainsKey(repo)) {
                PropertyChangedEventHandler handler = delegate(object sender, PropertyChangedEventArgs e) {
                    if (e.PropertyName == Utils.NameOf((Repository r) => r.Status)) {
                        this.AnyStatusChanged();
                    }
                };
                repo.PropertyChanged += handler;
                this.repos.Add(repo, handler);
            }
        }

        /// <summary>
        /// Remove the specified repo.
        /// </summary>
        /// <param name="repo">Repository to stop listening to.</param>
        public void Remove(Repository repo) {
            if (this.repos.ContainsKey(repo)) {
                repo.PropertyChanged -= this.repos[repo];
                this.repos.Remove(repo);
                this.AnyStatusChanged();
            }
        }

        /// <summary>
        /// This method is called by the Set accessor of each property.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        private void NotifyPropertyChanged(string propertyName) {
            if (string.IsNullOrEmpty(propertyName)) {
                throw new ArgumentNullException("Given property name is null");
            }

            var handler = this.PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void AnyStatusChanged() {
            bool allPaused = true;
            bool allDisconnected = true;
            bool anyWarning = false;
            bool anySyncing = false;
            foreach(var repo in this.repos.Keys) {
                switch (repo.Status) {
                case SyncStatus.Warning:
                    anyWarning = true;
                    goto default;
                case SyncStatus.Synchronizing:
                    anySyncing = true;
                    goto default;
                case SyncStatus.Suspend:
                    allDisconnected = false;
                    break;
                case SyncStatus.Disconnected:
                    allPaused = false;
                    break;
                default:
                    allPaused = false;
                    allDisconnected = false;
                    break;
                }
            }

            if (anyWarning) {
                // If any warning occurs => warning
                this.Status = SyncStatus.Warning;
            } else if (anySyncing) {
                // If any syncing occurs => syncing
                this.Status = SyncStatus.Synchronizing;
            } else if (allDisconnected) {
                this.Status = SyncStatus.Disconnected;
            } else if (allPaused) {
                this.Status = SyncStatus.Suspend;
            } else {
                this.Status = SyncStatus.Idle;
            }
        }
    }
}