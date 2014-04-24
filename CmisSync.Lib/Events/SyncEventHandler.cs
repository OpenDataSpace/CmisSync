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
using System;

using log4net;

namespace CmisSync.Lib.Events
{
    ///<summary>
    ///Base class for all Event-Handlers
    ///</summary>
    public abstract class SyncEventHandler : IComparable<SyncEventHandler>, IComparable
    {
        public abstract bool Handle(ISyncEvent e);

        ///<summary>
        ///May not be changed during runtime
        ///</summary>
        public virtual int Priority {
            get {
                return EventHandlerPriorities.GetPriority(this.GetType());
            }
        }

        public int CompareTo(SyncEventHandler other) {
            return Priority.CompareTo(other.Priority);
        }

        // CompareTo is implemented for Sorting EventHandlers
        // Equals is not implemented because EventHandler removal shall work by Object.Equals
        int IComparable.CompareTo(object obj) {
            if(!(obj is SyncEventHandler)){
                throw new ArgumentException("Argument is not a SyncEventHandler", "obj");
            }
            SyncEventHandler other = obj as SyncEventHandler;
            return this.CompareTo(other);
        }

        public override string ToString() {
            return this.GetType() + " with Priority " + this.Priority.ToString();
        }
    }

}

