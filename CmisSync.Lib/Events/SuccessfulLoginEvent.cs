using System;

namespace CmisSync.Lib.Events
{
    public class SuccessfulLoginEvent : ISyncEvent
    {
        public override string ToString ()
        {
            return string.Format ("[SuccessfulLoginEvent]");
        }
    }
}

