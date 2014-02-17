using CmisSync.Lib.Events;

namespace CmisSync.Lib.Sync.Strategy { 
    public class ContentChangeEventTransformer : ReportingSyncEventHandler {
        public static readonly int DEFAULT_PRIORITY = 1000;

        public override int Priority {
            get {
                return DEFAULT_PRIORITY;
            }
        }

        public override bool Handle(ISyncEvent e) {
            return false;
        }

        public ContentChangeEventTransformer(ISyncEventQueue queue): base(queue) {
        }

    }
}
