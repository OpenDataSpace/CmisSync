//-----------------------------------------------------------------------
// <copyright file="InvalidFolderNameFilter.cs" company="GRAU DATA AG">
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
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Invalid folder name filter.
    /// </summary>
    public class InvalidFolderNameFilter
    {
        /// <summary>
        /// Regular expression to check whether a folder name is valid or not.
        /// </summary>
        private static Regex invalidFolderNameRegex = new Regex("[" + Regex.Escape(new string(Path.GetInvalidPathChars()) + "\"?:/\\|<>*") + "]");

        /// <summary>
        /// Checks the path for containing invalid folder names.
        /// Reports every filtered event to the queue.
        /// </summary>
        /// <returns>
        /// true if the path contains invalid folder names.
        /// </returns>
        /// <param name='path'>
        /// Path to be checked for containing invalid folder names.
        /// </param>
        /// <param name='reason'>Reason for the invalid folder name, or empty string.</param>
        public virtual bool CheckPath(string path, out string reason)
        {
            if (string.IsNullOrEmpty(path)) {
                reason = "Given Path is null or empty";
                return true;
            } else if (invalidFolderNameRegex.IsMatch(path.Replace("/", string.Empty).Replace("\\", string.Empty))) {
                reason = string.Format("Path \"{0}\" contains one of the illegal characters \"{1}\"", path, invalidFolderNameRegex.ToString());
                return true;
            } else {
                reason = string.Empty;
                return false;
            }
        }
    }
}