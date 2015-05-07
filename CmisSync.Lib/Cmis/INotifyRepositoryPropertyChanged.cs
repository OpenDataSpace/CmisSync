//-----------------------------------------------------------------------
// <copyright file="INotifyRepositoryPropertyChanged.cs" company="GRAU DATA AG">
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

    /// <summary>
    /// Repository instance notifying about its changed properties
    /// </summary>
    public interface INotifyRepositoryPropertyChanged : INotifyPropertyChanged {
        /// <summary>
        /// Occurs when an exception should be shown to the user.
        /// </summary>
        event EventHandler<RepositoryExceptionEventArgs> ShowException;

        /// <summary>
        /// Gets the current status of the synchronization (paused or not).
        /// </summary>
        /// <value>The status.</value>
        SyncStatus Status { get; }

        /// <summary>
        /// Gets the last time when a sync was finished without detected changes.
        /// </summary>
        /// <value>The last finished sync.</value>
        DateTime? LastFinishedSync { get; }

        /// <summary>
        /// Gets the number of changes which are actually found on queue.
        /// </summary>
        /// <value>The number of changes.</value>
        int NumberOfChanges { get; }

        /// <summary>
        /// Gets the name of the synchronized folder, as found in the CmisSync XML configuration file.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the URL of the remote CMIS endpoint.
        /// </summary>
        Uri RemoteUrl { get; }

        /// <summary>
        /// Gets the path of the local synchronized folder.
        /// </summary>
        string LocalPath { get; }
    }
}