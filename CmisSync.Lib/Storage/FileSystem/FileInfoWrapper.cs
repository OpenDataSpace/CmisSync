//-----------------------------------------------------------------------
// <copyright file="FileInfoWrapper.cs" company="GRAU DATA AG">
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
using System.IO;
using System;


namespace CmisSync.Lib.Storage
{
    ///
    ///<summary>Wrapper for FileInfo<summary>
    ///
    public class FileInfoWrapper : FileSystemInfoWrapper, IFileInfo
    {
        private FileInfo original;

        public FileInfoWrapper(FileInfo fileInfo)
            : base(fileInfo)
        {
            original = fileInfo;
        }

        public IDirectoryInfo Directory {
            get {
                return new DirectoryInfoWrapper(original.Directory);
            }
        }

        public long Length {
            get {
                return original.Length;
            }
        }

        public DateTime LastWriteTime {
            get {
                return original.LastWriteTime;
            }
        }

        public Stream Open(FileMode mode)
        {
            return original.Open(mode);
        }

        public Stream Open(FileMode mode, FileAccess access)
        {
            return original.Open(mode, access);
        }

        public Stream Open(FileMode mode, FileAccess access, FileShare share)
        {
            return original.Open(mode, access, share);
        }
    }
}
