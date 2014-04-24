using System;

using DotCMIS.Client;
using DotCMIS.Enums;

namespace CmisSync.Lib.Events
{
    public class RemoteEvent : ISyncEvent
    {
        private IChangeEvent change;

        public IChangeEvent Change { get { return this.change; } }

        public string ObjectId { get { return this.change.ObjectId; } }

        public DotCMIS.Enums.ChangeType? Type { get { return this.change.ChangeType; } }

        public RemoteEvent (IChangeEvent change)
        {
            if(change == null)
                throw new ArgumentNullException("The given change event must not be null");
            this.change = change;
        }

        public override string ToString ()
        {
            return string.Format ("[RemoteEvent: ChangeType={0} ObjectId={1}]", Type, ObjectId);
        }
    }
}

