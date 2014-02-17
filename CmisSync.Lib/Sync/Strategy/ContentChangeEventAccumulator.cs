using DotCMIS.Client;
using CmisSync.Lib.Events;

namespace CmisSync.Lib.Sync.Strategy
{
    public class ContentChangeEventAccumulator : SyncEventHandler{
        /// this has to run before ContentChangeEventTransformer
        public static readonly int DEFAULT_PRIORITY = 2000;

        private ISession session;

        public override int Priority {
            get {
                return DEFAULT_PRIORITY;
            }
        }

        public override bool Handle (ISyncEvent e) {
            if(!(e is ContentChangeEvent)){
                return false;
            }

            var contentChangeEvent = e as ContentChangeEvent;
            if(contentChangeEvent.Type != DotCMIS.Enums.ChangeType.Deleted) {
                contentChangeEvent.UpdateObject(session);
            }
            return false;
        }

        public ContentChangeEventAccumulator(ISession session) {
            this.session = session;
        }
    }
}
