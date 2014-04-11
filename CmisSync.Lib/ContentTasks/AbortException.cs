using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CmisSync.Lib.ContentTasks
{
    public class AbortException : Exception
    {
        public AbortException() : base("Abort exception") { }
        public AbortException(string msg) : base(msg) { }
    }
}
