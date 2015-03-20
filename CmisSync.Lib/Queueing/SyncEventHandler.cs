//-----------------------------------------------------------------------
// <copyright file="SyncEventHandler.cs" company="GRAU DATA AG">
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
namespace CmisSync.Lib.Queueing {
    using System;

    using CmisSync.Lib.Events;

    using log4net;

    /// <summary>
    /// Base class for all Event-Handlers
    /// </summary>
    public abstract class SyncEventHandler : IComparable<SyncEventHandler>, IComparable {
        /// <summary>
        /// Gets the priority.
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        public virtual int Priority {
            get {
                return EventHandlerPriorities.GetPriority(this.GetType());
            }
        }

        /// <summary>
        /// Handle the specified e.
        /// </summary>
        /// <param name='e'>
        /// The event to handle.
        /// </param>
        /// <returns>
        /// true if handled
        /// </returns>
        public abstract bool Handle(ISyncEvent e);

        /// <summary>
        /// Compares to another instance
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that indicates whether this instance precedes, follows, or appears in the same position in the sort order as the value parameter.
        /// </returns>
        /// <param name='other'>
        /// The other instance.
        /// </param>
        public int CompareTo(SyncEventHandler other) {
            return this.Priority.CompareTo(other.Priority);
        }

        // CompareTo is implemented for Sorting EventHandlers
        // Equals is not implemented because EventHandler removal shall work by Object.Equals
        int IComparable.CompareTo(object obj) {
            if(!(obj is SyncEventHandler)) {
                throw new ArgumentException("Argument is not a SyncEventHandler", "obj");
            }

            SyncEventHandler other = obj as SyncEventHandler;
            return this.CompareTo(other);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Queueing.SyncEventHandler"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Queueing.SyncEventHandler"/>.
        /// </returns>
        public override string ToString() {
            return this.GetType() + " with Priority " + this.Priority.ToString();
        }
    }
}