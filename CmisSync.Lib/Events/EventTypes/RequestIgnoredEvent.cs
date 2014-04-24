//-----------------------------------------------------------------------
// <copyright file="RequestIgnoredEvent.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Events
{
    public class RequestIgnoredEvent : ISyncEvent
    {
        private ISyncEvent ignoredEvent;
        public ISyncEvent IgnoredEvent { get{ return ignoredEvent; } }
        private readonly string reason;
        public string Reason{ get{return this.reason; } }
        public RequestIgnoredEvent (ISyncEvent ignoredEvent, string reason = null, SyncEventHandler source = null)
        {
            if(ignoredEvent== null)
                throw new ArgumentNullException("The ignored event cannot be null");
            this.ignoredEvent = ignoredEvent;
            if(reason == null && source == null)
                throw new ArgumentNullException("There must be a reason or source given for the ignored event");
            this.reason = (reason!=null)? reason: "Event has been ignored by: " + source.ToString();
        }

        public override string ToString ()
        {
            return string.Format ("[RequestIgnoredEvent: IgnoredEvent={0} Reason={1}]", IgnoredEvent, Reason);
        }
    }
}

