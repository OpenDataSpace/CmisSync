//-----------------------------------------------------------------------
// <copyright file="BubbledEvent.cs" company="GRAU DATA AG">
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
    /// Bubbled event ca be used to add an event of given source to another Queue
    /// </summary>
    public class BubbledEvent : EncapsuledEvent
    {
        /// <summary>
        /// Creates a new BubbleEvent with an embedded event and context
        /// informations of the given event.
        /// </summary>
        /// <param name="source">Context Informations of the given event</param>
        /// <param name="e">An Event from another context. Must not be null</param>
        public BubbledEvent(object source, ISyncEvent e) : base(e)
        {
            this.Source = source;
        }

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public object Source { get; private set; }

        /// <summary>
        /// Returns the description of the source and the bubbled event
        /// </summary>
        /// <returns>The content of the encapsuled event</returns>
        public override string ToString()
        {
            return string.Format("Bubbled Event: From \"{0}\" with bubbled Event \"{1}\"", this.Source, this.Event.ToString());
        }
    }
}
