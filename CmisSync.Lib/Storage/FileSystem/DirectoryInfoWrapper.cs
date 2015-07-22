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

namespace CmisSync.Lib.Storage.FileSystem {
    using System;
    using System.IO;
    using System.Security.AccessControl;

    /// <summary>
    /// Wrapper for DirectoryInfo
    /// </summary>
    public class DirectoryInfoWrapper : FileSystemInfoWrapper, IDirectoryInfo {
        private DirectoryInfo original;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Storage.FileSystem.DirectoryInfoWrapper"/> class.
        /// Uses the given real instance and passes every future call to this instance.
        /// </summary>
        /// <param name="directoryInfo">Directory info.</param>
        public DirectoryInfoWrapper(DirectoryInfo directoryInfo)
            : base(directoryInfo) {
            this.original = directoryInfo;
        }

        /// <summary>
        /// Gets the Parent property of the original instance.
        /// </summary>
        /// <value>The parent.</value>
        public IDirectoryInfo Parent {
            get { return new DirectoryInfoWrapper(this.original.Parent); }
        }

        /// <summary>
        /// Gets the root of the directory.
        /// </summary>
        /// <value>The root.</value>
        public IDirectoryInfo Root {
            get { return new DirectoryInfoWrapper(this.original.Root); }
        }

        /// <summary>
        /// Passes the Create call to the original instance.
        /// </summary>
        public void Create() {
            this.original.Create();
        }

        /// <summary>
        /// Passes the Delete call to the originial instance.
        /// </summary>
        /// <param name="recursive">If set to <c>true</c> recursive.</param>
        public void Delete(bool recursive) {
            this.original.Delete(true);
        }

        /// <summary>
        /// Gets the directories of the original instance wrapped by new DirectoryInfoWrapper instances.
        /// </summary>
        /// <returns>The directories.</returns>
        public IDirectoryInfo[] GetDirectories() {
            DirectoryInfo[] originalDirs = this.original.GetDirectories();
            IDirectoryInfo[] wrappedDirs = new IDirectoryInfo[originalDirs.Length];
            for (int i = 0; i < originalDirs.Length; i++) {
                wrappedDirs[i] = new DirectoryInfoWrapper(originalDirs[i]);
            }

            return wrappedDirs;
        }

        /// <summary>
        /// Gets the files of the original instance wrapped by new FileInfoWrapper instances.
        /// </summary>
        /// <returns>The files.</returns>
        public IFileInfo[] GetFiles() {
            FileInfo[] originalFiles = this.original.GetFiles();
            IFileInfo[] wrappedFiles = new IFileInfo[originalFiles.Length];
            for (int i = 0; i < originalFiles.Length; i++) {
                wrappedFiles[i] = new FileInfoWrapper(originalFiles[i]);
            }

            return wrappedFiles;
        }

        /// <summary>
        /// Moves the directory to the destination directory path by using the original instance.
        /// </summary>
        /// <param name="destDirName">Destination directory path.</param>
        public void MoveTo(string destDirName) {
            this.original.MoveTo(destDirName);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Storage.FileSystem.DirectoryInfoWrapper"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Storage.FileSystem.DirectoryInfoWrapper"/>.</returns>
        public override string ToString() {
            return string.Format("[{0}]", this.original.FullName);
        }

        /// <summary>
        /// Tries to set permission to read write access to the directory and its children
        /// </summary>
        public void TryToSetReadWritePermissionRecursively() {
#if !__MonoCS__
            try {
                this.ReadOnly = false;
            } catch (Exception e) {
                Console.WriteLine(e);
            }

            foreach (var file in this.GetFiles()) {
                try {
                    file.ReadOnly = false;
                } catch (Exception e) {
                    Console.WriteLine(e);
                }
            }

            foreach (var dir in this.GetDirectories()) {
                dir.TryToSetReadWritePermissionRecursively();
            }
#endif
        }
    }
}