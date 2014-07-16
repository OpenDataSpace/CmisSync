//-----------------------------------------------------------------------
// <copyright file="AbstractFileFilter.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Filter
{
    using System;
    
    using CmisSync.Lib.Events;

    /// <summary>
    /// Abstract file filter. It takes an event queue make it possible to report any filtered event by requeueing an ignore Event to the queue
    /// </summary>
    public abstract class AbstractFileFilter : SyncEventHandler
    {
        /// <summary>
        /// The queue where the ignores should be reported to.
        /// </summary>
        protected readonly ISyncEventQueue Queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Filter.AbstractFileFilter"/> class.
        /// </summary>
        /// <param name='queue'>
        /// Queue where all filtered events should be reported to.
        /// </param>
        public AbstractFileFilter(ISyncEventQueue queue)
        {
            if (queue == null)
            {
                throw new ArgumentNullException("The given queue must not be null, bacause the Filters are reporting their filtered events to this queue");
            }

            this.Queue = queue;
        }
    }
}
