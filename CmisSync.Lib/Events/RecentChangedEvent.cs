using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CmisSync.Lib.Events
{
    public class RecentChangedEvent : ISyncEvent
    {
        public string Path { get; private set; }
        public DateTime ChangeTime { get; private set; }
        public RecentChangedEvent(String path) : this(path, DateTime.Now) { }

        public RecentChangedEvent(String Path, DateTime? ChangeTime)
        {
            this.Path = Path;
            this.ChangeTime = ChangeTime != null? (DateTime) ChangeTime: DateTime.Now;
        }


        public override string ToString()
        {
            return String.Format("RecentChangedEvent: {0} at {1}", Path, ChangeTime.ToLongTimeString());
        }
    }
}
