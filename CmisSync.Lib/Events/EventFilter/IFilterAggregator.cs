//-----------------------------------------------------------------------
// <copyright file="IFilterAggregator.cs" company="GRAU DATA AG">
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
    /// I filter aggregator.
    /// </summary>
    public interface IFilterAggregator
    {
        /// <summary>
        /// Gets the file names filter.
        /// </summary>
        /// <value>The file names filter.</value>
        IgnoredFileNamesFilter FileNamesFilter { get; }

        /// <summary>
        /// Gets the folder names filter.
        /// </summary>
        /// <value>The folder names filter.</value>
        IgnoredFolderNameFilter FolderNamesFilter { get; }

        /// <summary>
        /// Gets the invalid folder names filter.
        /// </summary>
        /// <value>The invalid folder names filter.</value>
        InvalidFolderNameFilter InvalidFolderNamesFilter { get; }

        /// <summary>
        /// Gets the ignored folder filter.
        /// </summary>
        /// <value>The ignored folder filter.</value>
        IgnoredFoldersFilter IgnoredFolderFilter { get; }
    }
}