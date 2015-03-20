
namespace CmisSync.Lib.Cmis {
    using System;

    public enum ExceptionLevel {
        Info,
        Warning,
        Fatal
    }

    public enum ExceptionType {
        Unknown,
        LocalSyncTargetDeleted
    }

    public class RepositoryExceptionEventArgs : EventArgs {
        public RepositoryExceptionEventArgs(ExceptionLevel level, ExceptionType type, Exception e = null) {
            this.Level = level;
            this.Exception = e;
            this.Type = type;
        }

        public ExceptionType Type { get; private set; }
        public ExceptionLevel Level { get; private set; }
        public Exception Exception { get; private set; }
    }
}