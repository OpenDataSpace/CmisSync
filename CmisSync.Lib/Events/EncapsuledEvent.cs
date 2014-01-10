using System;
using System.Collections.Generic;
using System.Text;

namespace CmisSync.Lib.Events
{
    /// <summary>
    /// Takes an ISyncEvent and combines it together with the source object.
    /// This could be used to take one event of an event queue and put it into
    /// another queue without loosing context informations which are implicit given
    /// on the source queue.
    /// </summary>
    public class EncapsuledEvent : ISyncEvent
    {
        /// <summary>
        /// Embedded Event
        /// </summary>
        public ISyncEvent Event { get; private set; }

        /// <summary>
        /// Creates a new EncapsuledEvent with an embedded event.
        /// </summary>
        /// <param name="e">An Event from another context. Must not be null</param>
        public EncapsuledEvent(ISyncEvent e)
        {
            if(e == null)
                throw new ArgumentNullException("A EncapsuledEvent needs a ISyncEvent as parameter, but null was given");
            this.Event = e;
        }

        /// <summary>
        /// Returns the description of the embedded event
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("EncapsuledEvent: with embedded event \"{0}\"", Event.ToString());
        }
    }
}
