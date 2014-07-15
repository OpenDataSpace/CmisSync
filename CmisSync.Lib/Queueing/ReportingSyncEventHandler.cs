//-----------------------------------------------------------------------
// <copyright file="ReportingSyncEventHandler.cs" company="GRAU DATA AG">
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
namespace CmisSync.Lib.Events
{
    using System;

    using log4net;

    /// <summary>
    /// Abstrace baseclass for all SyncEventHandlers which need the queue.
    /// </summary>
    /// <exception cref='ArgumentNullException'>
    /// Is thrown when an argument passed to a method is invalid because it is <see langword="null" /> .
    /// </exception>
    public abstract class ReportingSyncEventHandler : SyncEventHandler
    {
        /// <summary>
        /// The queue.
        /// </summary>
        protected readonly ISyncEventQueue Queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.ReportingSyncEventHandler"/> class.
        /// </summary>
        /// <param name='queue'>
        /// A reference to the queue.
        /// </param>
        /// <exception cref='ArgumentNullException'>
        /// Is thrown when an argument passed to a method is invalid because it is <see langword="null" /> .
        /// </exception>
        public ReportingSyncEventHandler(ISyncEventQueue queue) : base() {
            if(queue == null) {
                throw new ArgumentNullException("Given SyncEventQueue was null");
            }

            this.Queue = queue;
        }
    }
}