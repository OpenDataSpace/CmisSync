using System;
using System.IO;
using System.Runtime.Serialization;

namespace CmisSync.Lib.Storage
{
    [Serializable]
    public class ExtendedAttributeException : IOException
    {
        public ExtendedAttributeException() : base("ExtendedAttribute manipulation exception") {}
        public ExtendedAttributeException(string msg) : base(msg) {}
        public ExtendedAttributeException (string message, Exception inner) : base (message, inner) { }
        protected ExtendedAttributeException (SerializationInfo info, StreamingContext context) : base (info, context) { }
    }
}

