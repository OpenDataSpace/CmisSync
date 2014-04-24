//-----------------------------------------------------------------------
// <copyright file="EncapsuledEvent.cs" company="GRAU DATA AG">
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
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Takes an ISyncEvent and combines it together with the source object.
    /// This could be used to take one event of an event queue and put it into
    /// another queue without loosing context informations which are implicit given
    /// on the source queue.
    /// </summary>
    public class EncapsuledEvent : ISyncEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.EncapsuledEvent"/> class with an embedded event.
        /// </summary>
        /// <param name="e">An Event from another context. Must not be null</param>
        public EncapsuledEvent(ISyncEvent e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("A EncapsuledEvent needs a ISyncEvent as parameter, but null was given");
            }

            this.Event = e;
        }

        /// <summary>
        /// Gets the embedded Event
        /// </summary>
        public ISyncEvent Event { get; private set; }

        /// <summary>
        /// Returns the description of the embedded event
        /// </summary>
        /// <returns>A description and the toString of the embedded event</returns>
        public override string ToString()
        {
            return string.Format("EncapsuledEvent: with embedded event \"{0}\"", this.Event.ToString());
        }
    }
}
