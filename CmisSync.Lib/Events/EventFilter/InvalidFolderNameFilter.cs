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

    /// <summary>
    /// Invalid folder name filter.
    /// </summary>
    public class InvalidFolderNameFilter : AbstractFileFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.Filter.InvalidFolderNameFilter"/> class.
        /// </summary>
        /// <param name='queue'>
        /// Queue where filtered events will be reported to.
        /// </param>
        public InvalidFolderNameFilter(ISyncEventQueue queue) : base(queue)
        {
        }

        /// <summary>
        /// Handles the specified events which are containing paths.
        /// If the path contains invalid folder names, true is returned. Otherwise false.
        /// </summary>
        /// <param name='e'>
        /// Events to be checked.
        /// </param>
        /// <returns>
        /// <c>true</c> if the path contains an invalid folder name.
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
        /// Checks the path for containing invalid folder names.
        /// Reports every filtered event to the queue.
        /// </summary>
        /// <returns>
        /// true if the path contains invalid folder names.
        /// </returns>
        /// <param name='e'>
        /// Event which should be reported as filtered, if the path contains invalid folder names.
        /// </param>
        /// <param name='path'>
        /// Path to be checked for containing invalid folder names.
        /// </param>
        private bool CheckPath(ISyncEvent e, string path)
        {
            if (Utils.IsInvalidFolderName(path.Replace("/", string.Empty).Replace("\"", string.Empty), new List<string>()))
            {
                Queue.AddEvent(new RequestIgnoredEvent(e, source: this));
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
