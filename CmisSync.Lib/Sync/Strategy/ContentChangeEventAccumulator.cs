using CmisSync.Lib.Events;

namespace CmisSync.Lib.Sync.Strategy
{
    public class ContentChangeEventAccumulator : SyncEventHandler{
        /// this has to run before ContentChangeEventTransformer
        public static readonly int DEFAULT_PRIORITY = 2000;

        public override int Priority {
            get {
                return DEFAULT_PRIORITY;
            }
        }

        public override bool Handle (ISyncEvent e) {
            return false;
        }
    }
}
