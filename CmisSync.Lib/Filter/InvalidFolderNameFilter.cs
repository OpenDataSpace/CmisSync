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

namespace CmisSync.Lib.Filter {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Invalid folder name filter.
    /// </summary>
    public class InvalidFolderNameFilter {
        /// <summary>
        /// Regular expression to check whether a folder name is valid or not.
        /// </summary>
        private static Regex invalidFolderNameRegex = new Regex("[" + Regex.Escape(new string(Path.GetInvalidPathChars()) + "\"?:/\\|<>*") + "]");

        /// <summary>
        /// Checks the name of the folder.
        /// </summary>
        /// <returns><c>true</c>, if folder name contains invalid characters, <c>false</c> otherwise.</returns>
        /// <param name="name">Name of the folder.</param>
        /// <param name="reason">Reason why the answer was <c>true</c>.</param>
        public virtual bool CheckFolderName(string name, out string reason) {
            if (invalidFolderNameRegex.IsMatch(name)) {
                reason = string.Format("Folder name \"{0}\" contains one of the illegal characters \"{1}\"", name, invalidFolderNameRegex.ToString());
                return true;
            } else {
                reason = string.Empty;
                return false;
            }
        }
    }
}