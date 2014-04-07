using log4net;

using System;
using System.Collections.Generic;

namespace CmisSync.Lib.Events
{
    public class SyncEventManager
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(SyncEventManager));
        private List<SyncEventHandler> handler = new List<SyncEventHandler>();
        private readonly string Name;
        public SyncEventManager(string name = "SyncEventManager")
        {
            if(name == null) {
                throw new ArgumentNullException("Given name of the manager is null");
            }
            Name = name;
        }

        public void AddEventHandler(SyncEventHandler h)
        {
            logger.Debug("Adding Eventhandler " + h);
            //The zero-based index of item in the sorted List<T>, 
            //if item is found; otherwise, a negative number that 
            //is the bitwise complement of the index of the next 
            //element that is larger than item or.
            int pos = handler.BinarySearch(h);
            if(pos < 0){
                pos = ~pos;
            }
            handler.Insert(pos, h);
        }

        public virtual void Handle(ISyncEvent e) {
            using(log4net.ThreadContext.Stacks["NDC"].Push(Name))
            {
                for(int i = handler.Count-1; i >= 0; i--)
                {
                    var h = handler[i];
                    logger.Debug("Forwarding to Handler " + h);
                    if(handler[i].Handle(e)){
                        return;
                    }
                }
            }
        }

        public void RemoveEventHandler(SyncEventHandler h)
        {
            handler.Remove(h);
        }
    }
}

