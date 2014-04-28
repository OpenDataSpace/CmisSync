//-----------------------------------------------------------------------
// <copyright file="FileSystemInfoWrapper.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Storage
{
    ///
    ///<summary>Wrapper for DirectoryInfo<summary>
    ///
    public abstract class FileSystemInfoWrapper : IFileSystemInfo
    {
        private FileSystemInfo original;

        private static IExtendedAttributeReader reader = null;

        static FileSystemInfoWrapper()
        {
            switch (Environment.OSVersion.Platform)
            {
            case PlatformID.MacOSX:
                goto case PlatformID.Unix;
            case PlatformID.Unix:
                reader = new ExtendedAttributeReaderUnix();
                break;
            case PlatformID.Win32NT:
                reader = new ExtendedAttributeReaderDos();
                break;
            }
        }

        protected FileSystemInfoWrapper (FileSystemInfo original)
        {
            this.original = original;
        }

        public string FullName { get { return original.FullName; } }

        public string Name { get { return original.Name; } }

        public bool Exists { get { return original.Exists; } }

        public FileAttributes Attributes { get { return original.Attributes; } }

        public void Refresh ()
        {
            original.Refresh ();
        }

        public string GetExtendedAttribute(string key)
        {
            if(reader != null)
            {
                return reader.GetExtendedAttribute(original.FullName, key);
            }
            else
            {
                throw new ExtendedAttributeException("Feature is not supported");
            }
        }

        public void SetExtendedAttribute(string key, string value)
        {
            if(reader!=null)
            {
                reader.SetExtendedAttribute(original.FullName, key, value);
            }
            else
            {
                throw new ExtendedAttributeException("Feature is not supported");
            }
        }
    }
}
