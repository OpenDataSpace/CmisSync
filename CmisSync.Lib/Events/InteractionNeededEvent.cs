
namespace CmisSync.Lib.Events
{
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Consumer;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Exceptions;

    public class InteractionNeededEvent : ExceptionEvent
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string Details { get; set; }

        public Dictionary<string, Action> Actions { get; private set; }

        public List<IFileSystemInfo> AffectedFiles { get; private set; }

        public InteractionNeededEvent(InteractionNeededException e) : base(e)
        {
            if (e == null) {
                throw new ArgumentNullException("Given Exception is null");
            }

            this.AffectedFiles = new List<IFileSystemInfo>(e.AffectedFiles);
            this.Actions = new Dictionary<string, Action>(e.Actions);
            this.Title = e.Title;
            this.Description = e.Description;
            this.Details = e.Details;
        }

        public InteractionNeededEvent(string msg) : this(new InteractionNeededException(msg)) {
        }
    }
}