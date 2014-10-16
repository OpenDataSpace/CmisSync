

namespace CmisSync.Lib.SelectiveIgnore
{
    using System;
    using System.Collections.ObjectModel;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;

    public class SelectiveIgnoreEventTransformer : SyncEventHandler
    {
        private ISyncEventQueue queue;
        private ObservableCollection<IIgnoredEntity> ignores;

        public SelectiveIgnoreEventTransformer(ObservableCollection<IIgnoredEntity> ignores, ISyncEventQueue queue) {
            if (queue == null) {
                throw new ArgumentNullException("Given queue is empty");
            }

            if (ignores == null) {
                throw new ArgumentNullException("Given ignore collection is null");
            }

            this.queue = queue;
            this.ignores = ignores;
        }

        public override bool Handle(ISyncEvent e)
        {
            throw new NotImplementedException();
        }
    }
}