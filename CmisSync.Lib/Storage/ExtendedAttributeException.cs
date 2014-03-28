using System;
using System.IO;

namespace CmisSync.Lib.Storage
{
    public class ExtendedAttributeException : IOException
    {
        public ExtendedAttributeException() : base("ExtendedAttribute manipulation exception") {}
        public ExtendedAttributeException(string msg) : base(msg) {}
    }
}

