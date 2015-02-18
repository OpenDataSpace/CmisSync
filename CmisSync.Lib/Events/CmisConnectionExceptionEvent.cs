
namespace CmisSync.Lib.Events {
    using System;

    using DotCMIS.Exceptions;

    public class CmisConnectionExceptionEvent : ExceptionEvent, ICountableEvent {
        private readonly string category = "CmisConnectionException";
        public DateTime OccuredAt { get; private set; }
        public CmisConnectionExceptionEvent(CmisConnectionException connectionException) : base(connectionException) {
            this.OccuredAt = DateTime.Now;
        }

        public string Category {
            get {
                return this.category;
            }
        }
    }
}