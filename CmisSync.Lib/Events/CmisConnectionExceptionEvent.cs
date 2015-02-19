
namespace CmisSync.Lib.Events {
    using System;

    using DotCMIS.Exceptions;

    /// <summary>
    /// Cmis connection exception occured while handling event in SyncEventQueue and this event is added back to Queue to inform about this problem.
    /// </summary>
    public class CmisConnectionExceptionEvent : ExceptionEvent, ICountableEvent {
        private readonly string category = "CmisConnectionException";

        /// <summary>
        /// Gets the time when the exception occured.
        /// </summary>
        /// <value>The occured at this timestamp.</value>
        public DateTime OccuredAt { get; private set; }
        public CmisConnectionExceptionEvent(CmisConnectionException connectionException) : base(connectionException) {
            this.OccuredAt = DateTime.Now;
        }

        /// <summary>
        /// Gets the category of the event. This can be used to differ between multiple event types.
        /// The returned value should never ever change its value after requesting it the first time.
        /// </summary>
        /// <value>The event category.</value>
        public string Category {
            get {
                return this.category;
            }
        }
    }
}