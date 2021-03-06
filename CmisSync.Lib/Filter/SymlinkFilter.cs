//-----------------------------------------------------------------------
// <copyright file="SymlinkFilter.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Storage.FileSystem;

    /// <summary>
    /// Symlink filter.
    /// </summary>
    public class SymlinkFilter {
        /// <summary>
        /// Determines whether the given fsInfo instance is symlink and if not, the reason is set to an error description.
        /// </summary>
        /// <returns><c>true</c> if the given instance is symlink; otherwise, <c>false</c>.</returns>
        /// <param name="fsInfo">File system info.</param>
        /// <param name="reason">Reason for the filtering.</param>
        public bool IsSymlink(IFileSystemInfo fsInfo, out string reason) {
            reason = string.Empty;
            if (fsInfo != null && fsInfo.Exists && fsInfo.IsSymlink) {
                reason = string.Format("{0} is a symbolic link", fsInfo.FullName);
                return true;
            } else {
                return false;
            }
        }
    }
}