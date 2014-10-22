

namespace CmisSync.Lib.SelectiveIgnore
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;

    public class IgnoreFlagChangeDetection : SyncEventHandler
    {
        private ObservableCollection<IIgnoredEntity> ignores;
        public IgnoreFlagChangeDetection(ObservableCollection<IIgnoredEntity> ignores)
        {
            if (ignores == null) {
                throw new ArgumentNullException("Given ignores are null");
            }

            this.ignores = ignores;
        }

        public override bool Handle(ISyncEvent e)
        {
            if (e is ContentChangeEvent) {
                var change = e as ContentChangeEvent;
                if (this.IsIgnoredId(change.ObjectId) && change.CmisObject != null) {
                    var obj = change.CmisObject;
                    if (obj.AreAllChildrenIgnored()) {
                        return false;
                    } else {
//                        this.ignore
                    }
                }
            }

            return false;
        }

        private bool IsIgnoredId(string objectId) {
            foreach(var ignore in this.ignores) {
                if (objectId == ignore.ObjectId) {
                    return true;
                }
            }

            return false;
        }
    }
}