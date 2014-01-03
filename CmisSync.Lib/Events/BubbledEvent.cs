using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CmisSync.Lib.Events
{
    /// <summary>
    /// Takes an ISyncEvent and combines it together with the source object.
    /// This could be used to take one event of an event queue and put it into
    /// another queue without loosing context informations which are implicit given
    /// on the source queue.
    /// </summary>
    public class BubbledEvent : ISyncEvent
    {
        /// <summary>
        /// Source Informations
        /// </summary>
        public object Source { get; private set; }

        /// <summary>
        /// Original Event
        /// </summary>
        public ISyncEvent Event { get; private set; }

        /// <summary>
        /// Creates a new BubbleEvent with an embedded event and context
        /// informations of the given event.
        /// </summary>
        /// <param name="source">Context Informations of the given event</param>
        /// <param name="e">An Event from another context. Must not be null</param>
        public BubbledEvent(object source, ISyncEvent e)
        {
            if(e == null)
                throw new ArgumentNullException("A BubbledEvent needs a ISyncEvent as parameter, but null was given");
            this.Source = source;
            this.Event = e;
        }

        /// <summary>
        /// Returns the description of the source and the bubbled event
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("Bubbled Event: From \"{0}\" with bubbled Event \"{1}\"", Source, Event.ToString());
        }
    }
}
