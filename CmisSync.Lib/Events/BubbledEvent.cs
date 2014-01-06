using System;

namespace CmisSync.Lib.Events
{
    public class BubbledEvent : EncapsuledEvent
    {
        /// <summary>
        /// Source Informations
        /// </summary>
        public object Source { get; private set; }

        /// <summary>
        /// Creates a new BubbleEvent with an embedded event and context
        /// informations of the given event.
        /// </summary>
        /// <param name="source">Context Informations of the given event</param>
        /// <param name="e">An Event from another context. Must not be null</param>
        public BubbledEvent (object source, ISyncEvent e) : base(e)
        {
            this.Source = source;
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

