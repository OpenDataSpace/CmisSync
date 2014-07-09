//-----------------------------------------------------------------------
// <copyright file="ReportingFilter.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Events.Filter
{
    using System;

    /// <summary>
    /// Reporting filter.
    /// </summary>
    public class ReportingFilter : ReportingSyncEventHandler
    {
        /// <summary>
        /// The ignored folders filter.
        /// </summary>
        private IgnoredFoldersFilter ignoredFoldersFilter;

        /// <summary>
        /// The ignored file name filter.
        /// </summary>
        private IgnoredFileNamesFilter ignoredFileNameFilter;

        /// <summary>
        /// The ignored folder name filter.
        /// </summary>
        private IgnoredFolderNameFilter ignoredFolderNameFilter;

        /// <summary>
        /// The invalid folder name filter.
        /// </summary>
        private InvalidFolderNameFilter invalidFolderNameFilter;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.Filter.ReportingFilter"/> class.
        /// </summary>
        /// <param name="queue">Sync Event Queue to put work on.</param>
        /// <param name="ignoredFoldersFilter">Ignored folders filter.</param>
        /// <param name="ignoredFileNameFilter">Ignored file name filter.</param>
        /// <param name="ignoredFolderNameFilter">Ignored folder name filter.</param>
        public ReportingFilter(
            ISyncEventQueue queue,
            IgnoredFoldersFilter ignoredFoldersFilter,
            IgnoredFileNamesFilter ignoredFileNameFilter,
            IgnoredFolderNameFilter ignoredFolderNameFilter,
            InvalidFolderNameFilter invalidFoderNameFilter) : base(queue)
        {
            if (ignoredFoldersFilter == null) {
                throw new ArgumentNullException("Given folder filter is null");
            }

            if (ignoredFileNameFilter == null) {
                throw new ArgumentNullException("Given file name filter is null");
            }

            if (ignoredFolderNameFilter == null) {
                throw new ArgumentNullException("Given folder name filter is null");
            }

            if (invalidFoderNameFilter == null) {
                throw new ArgumentNullException("Given invalid folder name filter is null");
            }

            this.ignoredFoldersFilter = ignoredFoldersFilter;
            this.ignoredFileNameFilter = ignoredFileNameFilter;
            this.ignoredFolderNameFilter = ignoredFolderNameFilter;
            this.invalidFolderNameFilter = invalidFoderNameFilter;
        }

        /// <summary>
        /// Handle the specified e.
        /// </summary>
        /// <param name="e">The event to handle.</param>
        /// <returns>true if handled</returns>
        public override bool Handle(ISyncEvent e)
        {
            string reason;
            try {
                var nameEvent = e as IFilterableNameEvent;
                if (nameEvent != null && nameEvent.Name != null) {
                    if (nameEvent.IsDirectory) {
                        if (this.ignoredFolderNameFilter.CheckFolderName(nameEvent.Name, out reason)) {
                            this.Queue.AddEvent(new RequestIgnoredEvent(e, reason, this));
                            return true;
                        }
                    } else {
                        if (this.ignoredFileNameFilter.CheckFile(nameEvent.Name, out reason)) {
                            this.Queue.AddEvent(new RequestIgnoredEvent(e, reason, this));
                            return true;
                        }
                    }
                }

                var pathEvent = e as IFilterableRemotePathEvent;
                if (pathEvent != null && pathEvent.RemotePath != null) {
                    if (this.ignoredFoldersFilter.CheckPath(pathEvent.RemotePath, out reason)) {
                        this.Queue.AddEvent(new RequestIgnoredEvent(e, reason, this));
                        return true;
                    }

                    string[] folderNames = pathEvent.RemotePath.Split('/');
                    foreach(var name in folderNames) {
                        if (this.invalidFolderNameFilter.CheckFolderName(name, out reason)) {
                            this.Queue.AddEvent(new RequestIgnoredEvent(e, reason, this));
                            return true;
                        }
                    }
                }
            } catch (System.IO.DirectoryNotFoundException) {
            } catch (System.IO.FileLoadException) {
            } catch (System.NullReferenceException) {
            }

            return false;
        }
    }
}