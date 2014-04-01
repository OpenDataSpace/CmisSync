using System;
using System.Text;
using System.Collections.Generic;

#if __MonoCS__
using Mono.Unix.Native;
#endif

namespace CmisSync.Lib.Storage
{
    public class ExtendedAttributeReaderUnix : IExtendedAttributeReader
    {
        private readonly string prefix;

        public ExtendedAttributeReaderUnix(string prefix = "user.")
        {
#if __MonoCS__
            if(String.IsNullOrEmpty(prefix))
            {
                throw new ArgumentNullException("The given prefix is null or empty");
            }
            this.prefix = prefix;
#else
            throw new WrongPlatformException();
#endif
        }

        public string GetExtendedAttribute (string path, string key)
        {
#if __MonoCS__
            byte[] value;
            long ret = Syscall.getxattr(path, prefix + key, out value);
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
            throw new WrongPlatformException ();
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
                ret = Syscall.setxattr(path, prefix + key, Encoding.UTF8.GetBytes(value));
            }
            if(ret != 0)
            {
                throw new ExtendedAttributeException(Syscall.GetLastError().ToString());
            }
#else
            throw new WrongPlatformException ();
#endif

        }

        public void RemoveExtendedAttribute (string path, string key)
        {
#if __MonoCS__
            long ret = Syscall.removexattr (path, prefix + key);
            if(ret != 0)
            {
                throw new ExtendedAttributeException(Syscall.GetLastError().ToString());
            }
#else
            throw new WrongPlatformException ();
#endif
        }

        public List<string> ListAttributeKeys (string path)
        {
#if __MonoCS__
            string[] list;
            Syscall.listxattr(path, out list);
            List<string> result = new List<string>();
            foreach(string key in list)
            {
                if(key.StartsWith(prefix))
                {
                    result.Add(key.Substring(prefix.Length));
                }
            }
            return result;
#else
            throw new WrongPlatformException ();
#endif
        }
    }
}
