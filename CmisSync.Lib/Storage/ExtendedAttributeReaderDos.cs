using System;
using System.IO;
using Mono.Unix;

namespace CmisSync.Lib.Storage
{
    class ExtendedAttributeReaderDos : IExtendedAttributeReader
    {
        public string GetExtendedAttribute (string path, string key)
        {
            throw new NotImplementedException ();
        }

        public void SetExtendedAttribute (string path, string key, string value)
        {
            throw new NotImplementedException ();
        }

        public void RemoveExtendedAttribute (string path, string key)
        {
            throw new NotImplementedException ();
        }
    }

}
