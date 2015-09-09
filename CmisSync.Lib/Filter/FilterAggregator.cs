//-----------------------------------------------------------------------
// <copyright file="FilterAggregator.cs" company="GRAU DATA AG">
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

    /// <summary>
    /// Filter aggregator.
    /// </summary>
    public class FilterAggregator : IFilterAggregator {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Filter.FilterAggregator"/> class.
        /// </summary>
        /// <param name="fileNamesFilter">File names filter.</param>
        /// <param name="folderNamesFilter">Folder names filter.</param>
        /// <param name="invalidFolderNamesFilter">Invalid folder names filter.</param>
        /// <param name="ignoredFolderFilter">Ignored folder filter.</param>
        /// <param name="symlinkFilter">Symbolic link filter.</param>
        public FilterAggregator(
            IgnoredFileNamesFilter fileNamesFilter,
            IgnoredFolderNameFilter folderNamesFilter,
            InvalidFolderNameFilter invalidFolderNamesFilter,
            IgnoredFoldersFilter ignoredFolderFilter,
            SymlinkFilter symlinkFilter = null)
        {
            if (fileNamesFilter == null) {
                throw new ArgumentNullException("fileNamesFilter");
            }

            if (folderNamesFilter == null) {
                throw new ArgumentNullException("folderNamesFilter");
            }

            if (invalidFolderNamesFilter == null) {
                throw new ArgumentNullException("invalidFolderNamesFilter");
            }

            if (ignoredFolderFilter == null) {
                throw new ArgumentNullException("ignoredFolderFilter");
            }

            this.FileNamesFilter = fileNamesFilter;
            this.FolderNamesFilter = folderNamesFilter;
            this.InvalidFolderNamesFilter = invalidFolderNamesFilter;
            this.IgnoredFolderFilter = ignoredFolderFilter;
            this.SymlinkFilter = symlinkFilter ?? new SymlinkFilter();
        }
        
        /// <summary>
        /// Gets the file names filter.
        /// </summary>
        /// <value>The file names filter.</value>
        public IgnoredFileNamesFilter FileNamesFilter { get; private set; }

        /// <summary>
        /// Gets the folder names filter.
        /// </summary>
        /// <value>The folder names filter.</value>
        public IgnoredFolderNameFilter FolderNamesFilter { get; private set; }

        /// <summary>
        /// Gets the invalid folder names filter.
        /// </summary>
        /// <value>The invalid folder names filter.</value>
        public InvalidFolderNameFilter InvalidFolderNamesFilter { get; private set; }

        /// <summary>
        /// Gets the ignored folder filter.
        /// </summary>
        /// <value>The ignored folder filter.</value>
        public IgnoredFoldersFilter IgnoredFolderFilter { get; private set; }

        /// <summary>
        /// Gets the symlink filter.
        /// </summary>
        /// <value>The symlink filter.</value>
        public SymlinkFilter SymlinkFilter { get; private set; }
    }
}