//-----------------------------------------------------------------------
// <copyright file="ReadOnlyIgnoringDirectoryInfoDecorator.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Storage.FileSystem {
    using System;

    /// <summary>
    /// Read only ignoring directory info decorator.
    /// </summary>
    public class ReadOnlyIgnoringDirectoryInfoDecorator : ReadOnlyIgnoringFileSystemInfoDecorator, IDirectoryInfo {
        private IDirectoryInfo dirInfo;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="CmisSync.Lib.Storage.FileSystem.ReadOnlyIgnoringDirectoryInfoDecorator"/> class.
        /// </summary>
        /// <param name="dirInfo">Directory info.</param>
        public ReadOnlyIgnoringDirectoryInfoDecorator(IDirectoryInfo dirInfo) : base(dirInfo) {
            this.dirInfo = dirInfo;
        }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public IDirectoryInfo Parent { 
            get { return new ReadOnlyIgnoringDirectoryInfoDecorator(this.dirInfo.Parent); }
        }

        /// <summary>
        /// Gets the root of the directory.
        /// </summary>
        /// <value>The root.</value>
        public IDirectoryInfo Root { 
            get { return new ReadOnlyIgnoringDirectoryInfoDecorator(this.dirInfo.Root); }
        }

        /// <summary>
        /// Creates a directory.
        /// </summary>
        public void Create() {
            this.dirInfo.Create();
        }

        /// <summary>
        /// Gets the child directories.
        /// </summary>
        /// <returns>The directories.</returns>
        public IDirectoryInfo[] GetDirectories() {
            var dirs = this.dirInfo.GetDirectories();
            for (int i = 0; i < dirs.Length; i++) {
                dirs[i] = new ReadOnlyIgnoringDirectoryInfoDecorator(dirs[i]);
            }

            return dirs;
        }

        /// <summary>
        /// Gets the containing files.
        /// </summary>
        /// <returns>The files.</returns>
        public IFileInfo[] GetFiles() {
            var files = this.dirInfo.GetFiles();
            for (int i = 0; i < files.Length; i++) {
                files[i] = new ReadOnlyIgnoringFileInfoDecorator(files[i]);
            }

            return files;
        }

        /// <summary>
        /// Delete the specified directory recursive if <c>true</c>.
        /// </summary>
        /// <param name="recursive">Deletes recursive if set to <c>true</c>.</param>
        public void Delete(bool recursive) {
            this.dirInfo.Delete(recursive);
        }

        /// <summary>
        /// Moves the directory to the destination directory path.
        /// </summary>
        /// <param name="destDirName">Destination directory path.</param>
        public void MoveTo(string destDirName) {
            this.dirInfo.MoveTo(destDirName);
        }
    }
}