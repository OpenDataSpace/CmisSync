//-----------------------------------------------------------------------
// <copyright file="IgnoredFilesFilter.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Events;

    /// <summary>
    /// Ignored files filter.
    /// TODO Should be implemented in the future, if explicit files could be ignored
    /// </summary>
    public class IgnoredFilesFilter : AbstractFileFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.Filter.IgnoredFilesFilter"/> class.
        /// Throws Exception while it is implemented.
        /// </summary>
        /// <param name='queue'>
        /// Queue to report filtering actions.
        /// </param>
        public IgnoredFilesFilter(ISyncEventQueue queue) : base(queue)
        {
            throw new NotImplementedException("IgnoredFilesFilter is not yet implemented, because there is no possibility to ignore excact files");
        }

        /// <summary>
        /// Handle the specified event if it contains a filename of an ignored file.
        /// </summary>
        /// <param name='e'>
        /// Called if any event occures.
        /// </param>
        /// <returns>
        /// <c>true</c> if the file should be ignored.
        /// </returns>
        public override bool Handle(ISyncEvent e)
        {
            FileDownloadRequest request = e as FileDownloadRequest;
            if (request == null)
            {
                return false;
            }

            // TODO If files could be ignored, they should be filtered out here
            return false;
        }
    }
}
