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

namespace CmisSync.Lib.Events.Filter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Ignored file names filter.
    /// </summary>
    public class IgnoredFileNamesFilter
    {
        /// <summary>
        /// The wildcards of the ignored file names.
        /// </summary>
        private List<Regex> wildcards = new List<Regex>();

        /// <summary>
        /// The wild card lock for concurrent access.
        /// </summary>
        private object wildCardLock = new object();

        /// <summary>
        /// Sets the wildcards as strings and transforms them internally into Regex instances.
        /// </summary>
        /// <value>
        /// The wildcards.
        /// </value>
        public List<string> Wildcards
        {
            set
            {
                lock (this.wildCardLock)
                {
                    this.wildcards.Clear();
                    foreach (var wildcard in value)
                    {
                        this.wildcards.Add(Utils.IgnoreLineToRegex(wildcard));
                    }
                }
            }
        }

        /// <summary>
        /// Checks the filename for valid regex.
        /// </summary>
        /// <returns>
        /// The file.
        /// </returns>
        /// <param name='e'>
        /// If set to <c>true</c> e.
        /// </param>
        /// <param name='fileName'>
        /// If set to <c>true</c> file name.
        /// </param>
        public virtual bool CheckFile(string name, out string reason)
        {
            lock (this.wildCardLock)
            {
                reason = string.Empty;
                if (!Utils.WorthSyncing(name, new List<string>()))
                {
                    reason = string.Format("Invalid file name: {0}", name);
                    return true;
                }

                foreach (var wildcard in this.wildcards)
                {
                    if (wildcard.IsMatch(name))
                    {
                        reason = string.Format("filename matches: {0}", wildcard.ToString());
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
