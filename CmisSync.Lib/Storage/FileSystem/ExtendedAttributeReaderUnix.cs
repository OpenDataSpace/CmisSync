//-----------------------------------------------------------------------
// <copyright file="ExtendedAttributeReaderUnix.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

#if __MonoCS__
using Mono.Unix.Native;
#endif

namespace CmisSync.Lib.Storage
{
    public class ExtendedAttributeReaderUnix : IExtendedAttributeReader
    {
        private readonly string prefix = "user.";

        public ExtendedAttributeReaderUnix()
        {
#if (__MonoCS__ != true)
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
      
        public bool IsFeatureAvaillable()
        {
#if __MonoCS__
            byte[] value;
            string key = "test";
            string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            File.Create(path);
            long ret = Syscall.getxattr(path, prefix + key, out value);
            bool retValue = true;
            if(ret != 0)
            {
                Errno error = Syscall.GetLastError();
                if(error.ToString().Equals("EOPNOTSUPP")) {
                    retValue = false;
                }
            }
            if(File.Exists(path)) {
                File.Delete(path);
            }
            return retValue;
#else
            throw new WrongPlatformException ();
#endif
        }
    }
}
