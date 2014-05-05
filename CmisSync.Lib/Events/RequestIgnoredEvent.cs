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

