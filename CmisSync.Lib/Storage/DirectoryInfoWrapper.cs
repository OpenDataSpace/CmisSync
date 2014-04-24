//-----------------------------------------------------------------------
// <copyright file="DirectoryInfoWrapper.cs" company="GRAU DATA AG">
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
namespace CmisSync.Lib.Storage 
{
    ///
    ///<summary>Wrapper for DirectoryInfo<summary>
    ///
    public class DirectoryInfoWrapper : FileSystemInfoWrapper, IDirectoryInfo
    {
        private DirectoryInfo original; 

        public DirectoryInfoWrapper(DirectoryInfo directoryInfo) 
            : base(directoryInfo)
        {
            this.original = directoryInfo;
        }

        public void Create() {
            original.Create();
        }

        public void Delete (bool recursive) 
        {
            original.Delete(true);
        }

        public IDirectoryInfo Parent {
            get { 
                return new DirectoryInfoWrapper(original.Parent);
            } 
        }

        public IDirectoryInfo[] GetDirectories() {
            DirectoryInfo[] originalDirs = original.GetDirectories();
            IDirectoryInfo[] wrappedDirs = new IDirectoryInfo[originalDirs.Length];
            for(int i = 0; i < originalDirs.Length; i++){
                wrappedDirs[i] = new DirectoryInfoWrapper(originalDirs[i]);
            }
            return wrappedDirs;
        }

        public IFileInfo[] GetFiles() {
            FileInfo[] originalFiles = original.GetFiles();
            IFileInfo[] wrappedFiles = new IFileInfo[originalFiles.Length];
            for(int i = 0; i < originalFiles.Length; i++){
                wrappedFiles[i] = new FileInfoWrapper(originalFiles[i]);
            }
            return wrappedFiles;
        }
    }
}
