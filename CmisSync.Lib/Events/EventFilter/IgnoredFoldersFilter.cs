//-----------------------------------------------------------------------
// <copyright file="IgnoredFoldersFilter.cs" company="GRAU DATA AG">
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

    /// <summary>
    /// Ignored folders filter.
    /// </summary>
    public class IgnoredFoldersFilter : AbstractFileFilter
    {
        /// <summary>
        /// The ignored paths.
        /// </summary>
        private List<string> ignoredPaths = new List<string>();

        /// <summary>
        /// The wildcards.
        /// </summary>
        private List<string> wildcards = new List<string>();

        /// <summary>
        /// The wildcards list lock.
        /// </summary>
        private object listLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.Filter.IgnoredFoldersFilter"/> class.
        /// </summary>
        /// <param name='queue'>
        /// Queue where the reasons are reported to.
        /// </param>
        public IgnoredFoldersFilter(ISyncEventQueue queue) : base(queue)
        {
        }

        /// <summary>
        /// Sets the ignored paths.
        /// </summary>
        /// <value>
        /// The ignored paths.
        /// </value>
        public List<string> IgnoredPaths
        {
            set
            {
                lock (this.listLock)
                {
                    this.ignoredPaths = value;
                }
            }
        }

        /// <summary>
        /// Sets the ignore wildcards.
        /// </summary>
        /// <value>
        /// The ignore wildcards.
        /// </value>
        public List<string> IgnoreWildcards
        {
            set
            {
                lock (this.listLock)
                {
                    this.wildcards = value;
                }
            }
        }

        /// <summary>
        /// Handles FSEvents and FileDownloadRequest events.
        /// If the path starts with an ignored path, <c>true</c> will be returned and an ignored event is added to the queue.
        /// Otherwise <c>false</c> is returned.
        /// </summary>
        /// <param name='e'>
        /// Is checked for FSEvent events and FileDownloadRequest events
        /// </param>
        /// <returns>
        /// <c>true</c> if this event occured on an ignored path, otherwise <c>false</c>
        /// </returns>
        public override bool Handle(ISyncEvent e)
        {
            FileDownloadRequest request = e as FileDownloadRequest;
            if (request != null)
            {
                return this.CheckPath(request, request.LocalPath);
            }

            IFSEvent fsevent = e as IFSEvent;
            if (fsevent != null)
            {
                return this.CheckPath(fsevent, fsevent.Path);
            }

            return false;
        }

        /// <summary>
        /// Checks the path if it begins with any path, which is ignored. Reports ignores to the queue.
        /// </summary>
        /// <returns>
        /// <c>true</c> if path starts with an ignored path, otherwise <c>false</c> is returned.
        /// </returns>
        /// <param name='e'>
        /// ISyncEvent which is reported to queue, if filtered.
        /// </param>
        /// <param name='localPath'>
        /// The local path which should be checked, if it should be ignored.
        /// </param>
        private bool CheckPath(ISyncEvent e, string localPath)
        {
            lock (this.listLock)
            {
                bool result = !string.IsNullOrEmpty(this.ignoredPaths.Find(delegate(string ignore)
                                                                           {
                    return localPath.StartsWith(ignore);
                }));
                if (result)
                {
                    Queue.AddEvent(new RequestIgnoredEvent(e, source: this));
                }

                return result;
            }
        }
    }
}
