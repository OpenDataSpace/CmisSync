using CmisSync.Lib.Events;

using System.Collections.Generic;

namespace TestLibrary.TestUtils
{
    public class ObservableHandler : SyncEventHandler {
        public List<ISyncEvent> list = new List<ISyncEvent>();

        public override bool Handle(ISyncEvent e)
        {
            list.Add(e);
            return true;
        }

        public override int Priority
        {
            get {return 1;}
        }
    }
}
