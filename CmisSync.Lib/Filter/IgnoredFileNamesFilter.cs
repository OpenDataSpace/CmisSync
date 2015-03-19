//-----------------------------------------------------------------------
// <copyright file="IgnoredFileNamesFilter.cs" company="GRAU DATA AG">
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
    /// Ignored file names filter.
    /// </summary>
    public class IgnoredFileNamesFilter {
        /// <summary>
        /// The required wildcards are at the moment *.sync files
        /// </summary>
        private readonly string[] requiredWildcards = new string[] { "*.sync" };

        /// <summary>
        /// The wildcards of the ignored file names.
        /// </summary>
        private List<Regex> wildcards = new List<Regex>();

        /// <summary>
        /// The wild card lock for concurrent access.
        /// </summary>
        private object wildCardLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Filter.IgnoredFileNamesFilter"/> class.
        /// </summary>
        public IgnoredFileNamesFilter() {
            foreach (var required in this.requiredWildcards) {
                this.wildcards.Add(Utils.IgnoreLineToRegex(required));
            }
        }

        /// <summary>
        /// Sets the wildcards as strings and transforms them internally into Regex instances.
        /// </summary>
        /// <value>
        /// The wildcards.
        /// </value>
        public List<string> Wildcards {
            set {
                lock (this.wildCardLock) {
                    this.wildcards.Clear();
                    foreach (var required in this.requiredWildcards) {
                        if (!value.Contains(required)) {
                            this.wildcards.Add(Utils.IgnoreLineToRegex(required));
                        }
                    }

                    foreach (var wildcard in value) {
                        this.wildcards.Add(Utils.IgnoreLineToRegex(wildcard));
                    }
                }
            }
        }

        /// <summary>
        /// Checks the filename for valid regex.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the file should be ignored, otherwise <c>false</c>.
        /// </returns>
        /// <param name='name'>
        /// The file name
        /// </param>
        /// <param name='reason'>
        /// Is set to the reason if <c>true</c> is returned.
        /// </param>
        public virtual bool CheckFile(string name, out string reason) {
            lock (this.wildCardLock) {
                reason = string.Empty;
                if (!Utils.WorthSyncing(name, new List<string>())) {
                    reason = string.Format("Invalid file name: {0}", name);
                    return true;
                }

                foreach (var wildcard in this.wildcards) {
                    if (wildcard.IsMatch(name)) {
                        reason = string.Format("filename {1} matches: {0}", wildcard.ToString(), name);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}