
namespace CmisSync.Lib.Events
{
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Exceptions;

    public class InteractionNeededEvent : ExceptionEvent
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string Details { get; set; }

        public Dictionary<string, Action> Actions { get; private set; }

        public List<IFileSystemInfo> AffectedFiles { get; private set; }

        public InteractionNeededEvent(Exception e) : base(e)
        {
            this.AffectedFiles = new List<IFileSystemInfo>();
            this.Actions = new Dictionary<string, Action>();
            this.Title = e.GetType().Name;
            this.Description = e.Message;
            if (e is CmisBaseException) {
                this.Details = (e as CmisBaseException).ErrorContent;
            } else {
                this.Details = e.StackTrace ?? string.Empty;
            }
        }

        public InteractionNeededEvent(string msg) : this(new Exception(msg))
        {
        }
    }
}