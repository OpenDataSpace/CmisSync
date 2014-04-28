//-----------------------------------------------------------------------
// <copyright file="IFileInfo.cs" company="GRAU DATA AG">
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
    ///<summary>Interface to enable mocking of FileInfo<summary>
    ///
    public interface IFileInfo : IFileSystemInfo
    {
        IDirectoryInfo Directory { get; }
        DateTime LastWriteTimeUtc { get; }
        DateTime LastWriteTime { get; }
        long Length { get; }
        Stream Open (FileMode open);
        Stream Open (FileMode open, FileAccess access);
        Stream Open (FileMode open, FileAccess access, FileShare share);
    }
}
