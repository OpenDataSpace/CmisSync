using System;
using System.Text;

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
            byte[] value;
            long ret = Syscall.getxattr(path, "user." + key, out value);
            if(ret != 0)
            {
                Errno error = Syscall.GetLastError();
                if(error.ToString().Equals("ENODATA")) {
                    return null;
                } else {
                    //throw new ExtendedAttributeException(Syscall.GetLastError().ToString());
                }
            }
            if(value == null)
            {
                return null;
            }
            else
            {
                return Encoding.UTF8.GetString(value);
            }
#else
            throw new NotImplementedException ();
#endif

        }

        public void SetExtendedAttribute (string path, string key, string value)
        {
#if __MonoCS__
            long ret;
            if(value == null)
            {
                RemoveExtendedAttribute(path, key);
                return;
            }
            else
            {
                ret = Syscall.setxattr(path, "user." + key, Encoding.UTF8.GetBytes(value));
            }
            if(ret != 0)
            {
                throw new ExtendedAttributeException(Syscall.GetLastError().ToString());
            }
#else
            throw new NotImplementedException ();
#endif

        }

        public void RemoveExtendedAttribute (string path, string key)
        {
#if __MonoCS__
            long ret = Syscall.removexattr (path, "user." + key);
            if(ret != 0)
            {
                throw new ExtendedAttributeException(Syscall.GetLastError().ToString());
            }
#else
            throw new NotImplementedException ();
#endif
        }

        public string[] ListExtendedAttributes (string path)
        {
#if __MonoCS__
            string[] list;
            Syscall.listxattr(path, out list);
            return list;
#else
            throw new NotImplementedException ();
#endif
        }
    }
}
