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

namespace CmisSync.Lib.Events.Filter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Ignored folder name filter.
    /// </summary>
    public class IgnoredFolderNameFilter : AbstractFileFilter
    {
        /// <summary>
        /// The lock to prevent multiple parallel access on the wildcard list
        /// </summary>
        private object listLock = new object();

        /// <summary>
        /// The list of all wildcard regexes.
        /// </summary>
        private List<Regex> wildcards = new List<Regex>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.Filter.IgnoredFolderNameFilter"/> class.
        /// </summary>
        /// <param name='queue'>
        /// Queue where ignores and their reasons are reported to.
        /// </param>
        public IgnoredFolderNameFilter(ISyncEventQueue queue) : base(queue)
        {
        }

        /// <summary>
        /// Sets the wildcards.
        /// </summary>
        /// <value>
        /// The wildcards.
        /// </value>
        public List<string> Wildcards
        {
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Given wildcards are null");
                }

                lock (this.listLock)
                {
                    this.wildcards.Clear();
                    foreach (string wildcard in value)
                    {
                        this.wildcards.Add(Utils.IgnoreLineToRegex(wildcard));
                    }
                }
            }
        }

        /// <summary>
        /// Handle the passed events and filters all FSEvents which containing a matching folder name.
        /// </summary>
        /// <param name='e'>
        /// Event to be checked
        /// </param>
        /// <returns>
        /// <c>true</c> if folder name matches any wildcard, otherwise <c>false</c>
        /// </returns>
        public override bool Handle(ISyncEvent e)
        {
            if (e is IFilterableNameEvent)
            {
                IFilterableNameEvent filterable = e as IFilterableNameEvent;
                return this.CheckFolderName(e, filterable.Name);
            } else if (e is IFilterablePathEvent) {
                IFilterablePathEvent filterable = e as IFilterablePathEvent;
                string path = (e as IFilterablePathEvent).Path;


                if (!filterable.IsDirectory()) {
                    return this.CheckFolderName(e, Path.GetDirectoryName(path));
                }
            }

            return false;
        }

        private bool CheckFolderName(ISyncEvent e, string name) {
            lock (this.listLock)
            {
                foreach (Regex wildcard in this.wildcards)
                {
                    if (wildcard.IsMatch(name))
                    {
                        Queue.AddEvent(new RequestIgnoredEvent(e, string.Format("Folder \"{0}\" matches regex {1}", name, wildcard.ToString()), this));
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
