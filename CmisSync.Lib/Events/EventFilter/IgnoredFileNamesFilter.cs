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
    public class IgnoredFileNamesFilter : AbstractFileFilter
    {
        /// <summary>
        /// The wildcards of the ignored file names.
        /// </summary>
        private List<Regex> wildcards = new List<Regex>();

        /// <summary>
        /// The wild card lock for concurrent access.
        /// </summary>
        private Object wildCardLock = new Object();

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.Filter.IgnoredFileNamesFilter"/> class.
        /// </summary>
        /// <param name='queue'>
        /// Queue where the filter should report its reason of the filtering.
        /// </param>
        public IgnoredFileNamesFilter(ISyncEventQueue queue) : base(queue)
        {
        }

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
        /// Handles FSEvents and FileDownloadRequest events.
        /// </summary>
        /// <param name='e'>
        /// If a filename contains invalid patterns, <c>true</c> is returned and the filtering is reported to the queue. Otherwise <c>false</c> is returned.
        /// </param>
        /// <returns>
        /// <c>true</c> if any regex matches the file name, otherwise <c>false</c>
        /// </returns>
        public override bool Handle(ISyncEvent e)
        {
            FileDownloadRequest request = e as FileDownloadRequest;
            if (request != null)
            {
                return this.CheckFile(request, request.Document.Name);
            }
                
            FSEvent fsevent = e as FSEvent;
            if (fsevent != null)
            {
                try
                {
                    if (!fsevent.IsDirectory())
                    {
                        return this.CheckFile(fsevent, Path.GetFileName(fsevent.Path));
                    }
                }
                catch (System.IO.FileNotFoundException)
                {
                    // Only happens, if the deleted file/folder does not exists anymore
                    // To be sure, this event is not misinterpreted, just let it pass
                    return false;
                }
                catch (System.IO.DirectoryNotFoundException)
                {
                    return false;
                }
            }

            return false;
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
        private bool CheckFile(ISyncEvent e, string fileName)
        {
            lock (this.wildCardLock)
            {
                if (!Utils.WorthSyncing(fileName, new List<string>()))
                {
                    Queue.AddEvent(new RequestIgnoredEvent(e, source: this));
                    return true;
                }

                foreach (var wildcard in this.wildcards)
                {
                    if (wildcard.IsMatch(fileName))
                    {
                        Queue.AddEvent(new RequestIgnoredEvent(e, reason: String.Format("filename matches: {0}", wildcard.ToString()), source: this));
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
