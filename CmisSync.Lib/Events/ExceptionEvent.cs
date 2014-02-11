using System;

namespace CmisSync.Lib.Events
{
    public class ExceptionEvent : ISyncEvent
    {
        public Exception Exception;
        public ExceptionEvent (Exception e)
        {
            if(e == null)
                throw new ArgumentNullException("Given Exception is null");
            Exception = e;
        }

        public override string ToString ()
        {
            return Exception.Message;
        }
    }
}

