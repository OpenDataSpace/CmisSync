//-----------------------------------------------------------------------
// <copyright file="FileSystemInfoFactory.cs" company="GRAU DATA AG">
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
    ///<summary>Wrapps all interfaced methods and calls the Systems.IO classes<summary>
    ///
    public class FileSystemInfoFactory : IFileSystemInfoFactory
    {
        public IDirectoryInfo CreateDirectoryInfo (string path)
        {
            return new DirectoryInfoWrapper (new DirectoryInfo (path));
        }

        public IFileInfo CreateFileInfo (string path)
        {
            return new FileInfoWrapper (new FileInfo (path));
        }
    }
}
