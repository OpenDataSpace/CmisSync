//-----------------------------------------------------------------------
// <copyright file="AbstractEventProducer.cs" company="GRAU DATA AG">
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

    /// <summary>
    /// Abstract event producer.
    /// </summary>
    public abstract class AbstractEventProducer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.AbstractEventProducer"/> class.
        /// </summary>
        /// <param name='queue'>
        /// The queue which could be used to pass events to.
        /// </param>
        public AbstractEventProducer(ISyncEventQueue queue)
        {
            if (queue == null)
            {
                throw new ArgumentNullException("The given event queue must no be null");
            }

            this.Queue = queue;
        }

        /// <summary>
        /// Gets the queue where events can be added.
        /// </summary>
        /// <value>
        /// The queue.
        /// </value>
        protected ISyncEventQueue Queue { get; private set; }
    }
}
