using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using log4net;

namespace CmisSync.Lib.Events
{
    public interface ISyncEventQueue {

        /// <exception cref="InvalidOperationException">When Listener is already stopped</exception>
        void AddEvent(ISyncEvent newEvent); 

        bool IsStopped{get;} 
    }
}
