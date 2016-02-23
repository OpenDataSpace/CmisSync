//-----------------------------------------------------------------------
// <copyright file="IgnoredFolderNameFilter.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Storage.FileSystem;

    /// <summary>
    /// Ignored folder name filter.
    /// </summary>
    public class IgnoredFolderNameFilter {
        private string basePath;
        /// <summary>
        /// The lock to prevent multiple parallel access on the wildcard list
        /// </summary>
        private object listLock = new object();

        /// <summary>
        /// The list of all wildcard regexes.
        /// </summary>
        private List<Regex> wildcards = new List<Regex>();

        /// <summary>
        /// Sets the wildcards.
        /// </summary>
        /// <value>
        /// The wildcards.
        /// </value>
        public List<string> Wildcards {
            set {
                if (value == null) {
                    throw new ArgumentNullException("Wildcards");
                }

                lock (this.listLock) {
                    this.wildcards.Clear();
                    foreach (string wildcard in value) {
                        this.wildcards.Add(Utils.IgnoreLineToRegex(wildcard));
                    }
                }
            }
        }

        /// <summary>
        /// Checks the folder path. And returns true if path contains a name with any ignored regex.
        /// </summary>
        /// <returns><c>true</c>, if folder path should be ignored, <c>false</c> otherwise.</returns>
        /// <param name="localPath">Local path.</param>
        /// <param name="reason">Reason for the ignored state.</param>
        public virtual bool CheckFolderPath(string localPath, out string reason) {
            reason = string.Empty;
            if (localPath != null) {
                if (localPath.StartsWith(basePath)) {
                    string relativePath = localPath.Substring(basePath.Length).TrimStart(Path.DirectorySeparatorChar);
                    foreach (string folderName in relativePath.Split(Path.DirectorySeparatorChar)) {
                        bool check = CheckFolderName(folderName, out reason);
                        if (check) {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Filter.IgnoredFolderNameFilter"/> class.
        /// </summary>
        /// <param name="localBasePath">Local base path. All folder names below this base path won't be checked.</param>
        public IgnoredFolderNameFilter(IDirectoryInfo localBasePath) {
            if (localBasePath == null) {
                throw new ArgumentNullException("localBasePath");
            }

            this.basePath = localBasePath.FullName;
        }

        /// <summary>
        /// Checks the name of the folder.
        /// </summary>
        /// <returns><c>true</c>, if folder name should be ignored, <c>false</c> otherwise.</returns>
        /// <param name="name">Name of the folder.</param>
        /// <param name="reason">Reason why <c>true</c> was returned.</param>
        public virtual bool CheckFolderName(string name, out string reason) {
            lock (this.listLock) {
                reason = string.Empty;
                foreach (Regex wildcard in this.wildcards) {
                    if (wildcard.IsMatch(name)) {
                        reason = string.Format("Folder \"{0}\" matches regex {1}", name, wildcard.ToString());
                        return true;
                    }
                }
            }

            return false;
        }
    }
}