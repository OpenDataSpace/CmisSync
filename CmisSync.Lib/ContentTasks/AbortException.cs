using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace CmisSync.Lib.ContentTasks
{
    [Serializable]
    public class AbortException : Exception
    {
        public AbortException() : base("Abort exception") { }
        public AbortException(string msg) : base(msg) { }
        public AbortException (string message, Exception inner) : base (message, inner) { }
        protected AbortException (SerializationInfo info, StreamingContext context) : base (info, context) { }
    }
}
