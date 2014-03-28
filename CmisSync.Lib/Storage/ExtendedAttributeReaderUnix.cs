using System;

#if __MonoCS__
using Mono.Unix.Native;
#endif

namespace CmisSync.Lib.Storage
{
    public class ExtendedAttributeReaderUnix : IExtendedAttributeReader
    {
        public string GetExtendedAttribute (string path, string key)
        {
#if __MonoCS__
            //Syscall.getxattr();
#else

#endif
            throw new NotImplementedException ();
        }

        public void SetExtendedAttribute (string path, string key, string value)
        {
            throw new NotImplementedException ();
        }
    }
}
