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
        private Dictionary<INotifyRepositoryPropertyChanged, PropertyChangedEventHandler> repos = new Dictionary<INotifyRepositoryPropertyChanged, PropertyChangedEventHandler>();
        private SyncStatus status = SyncStatus.Disconnected;
        private int changesFound = 0;
        private DateTime? lastFinishedSync = null;

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

        public DateTime? LastFinishedSync {
            get {
                return this.lastFinishedSync;
            }

            private set {
                if (value != this.lastFinishedSync && value != null && value.GetValueOrDefault() > this.lastFinishedSync.GetValueOrDefault()) {
                    this.lastFinishedSync = value;
                    this.NotifyPropertyChanged(Utils.NameOf(() => this.LastFinishedSync));
                }
            }
        }

        /// <summary>
        /// Add the specified repo.
        /// </summary>
        /// <param name="repo">Repository to listen to.</param>
        public void Add(INotifyRepositoryPropertyChanged repo) {
            if (!this.repos.ContainsKey(repo)) {
                PropertyChangedEventHandler handler = delegate(object sender, PropertyChangedEventArgs e) {
                    if (e.PropertyName == Utils.NameOf((INotifyRepositoryPropertyChanged r) => r.Status)) {
                        this.AnyStatusChanged();
                    } else if (e.PropertyName == Utils.NameOf((INotifyRepositoryPropertyChanged r) => r.NumberOfChanges)) {
                        this.AnyNumberChanged();
                    }
                };
                repo.PropertyChanged += handler;
                this.repos.Add(repo, handler);
                if (repo.NumberOfChanges > 0) {
                    this.AnyNumberChanged();
                }

                this.AnyStatusChanged();
                this.AnyDateChanged();
            }
        }

        /// <summary>
        /// Remove the specified repo.
        /// </summary>
        /// <param name="repo">Repository to stop listening to.</param>
        public void Remove(INotifyRepositoryPropertyChanged repo) {
            if (this.repos.ContainsKey(repo)) {
                repo.PropertyChanged -= this.repos[repo];
                this.repos.Remove(repo);
                this.AnyStatusChanged();
                this.AnyNumberChanged();
                this.AnyDateChanged();
            }
        }

        /// <summary>
        /// Gets the number of changes which are actually found on queue.
        /// </summary>
        /// <value>The number of changes.</value>
        public int NumberOfChanges {
            get {
                return this.changesFound;
            }

            private set {
                if (value != this.changesFound) {
                    this.changesFound = value;
                    this.NotifyPropertyChanged(Utils.NameOf(() => this.NumberOfChanges));
                }
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

        private void AnyNumberChanged() {
            int sum = 0;
            foreach (var repo in repos.Keys) {
                sum += repo.NumberOfChanges;
            }

            this.NumberOfChanges = sum;
        }

        private void AnyDateChanged() {
            foreach (var repo in repos.Keys) {
                this.LastFinishedSync = repo.LastFinishedSync;
            }
        }
    }
}