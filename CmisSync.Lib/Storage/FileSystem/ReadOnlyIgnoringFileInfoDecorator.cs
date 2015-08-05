//-----------------------------------------------------------------------
// <copyright file="ReadOnlyIgnoringFileInfoDecorator.cs" company="GRAU DATA AG">
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

    /// <summary>
    /// Read only ignoring file info decorator decorades the given IFileInfo instance and removes read only flag before executing the operation and adds it back after successful operation.
    /// </summary>
    public class ReadOnlyIgnoringFileInfoDecorator : ReadOnlyIgnoringFileSystemInfoDecorator, IFileInfo {
        private IFileInfo fileInfo;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="CmisSync.Lib.Storage.FileSystem.ReadOnlyIgnoringFileInfoDecorator"/> class.
        /// </summary>
        /// <param name="fileInfo">File info.</param>
        public ReadOnlyIgnoringFileInfoDecorator(IFileInfo fileInfo) : base(fileInfo) {
            this.fileInfo = fileInfo;
        }

        /// <summary>
        /// Gets the parent directory.
        /// </summary>
        /// <value>The parent directory.</value>
        public IDirectoryInfo Directory {
            get { return this.fileInfo.Directory; }
        }

        /// <summary>
        /// Gets the file content length.
        /// </summary>
        /// <value>The length.</value>
        public long Length {
            get { return this.fileInfo.Length; }
        }

        /// <summary>
        /// Open the specified file.
        /// </summary>
        /// <param name="open">Open mode.</param>
        /// <returns>Stream of the content.</returns>
        public Stream Open(FileMode open) {
            return this.fileInfo.Open(open);
        }

        /// <summary>
        /// Open the specified file with the open and access mode.
        /// </summary>
        /// <param name="open">Open mode.</param>
        /// <param name="access">Access Mode.</param>
        /// <returns>Stream of the content</returns>
        public Stream Open(FileMode open, FileAccess access) {
            return this.fileInfo.Open(open, access);
        }

        /// <summary>
        /// Open the specified file with given open, access and share mode.
        /// </summary>
        /// <param name="open">Open mode.</param>
        /// <param name="access">Access mode.</param>
        /// <param name="share">Share mode.</param>
        /// <returns>Stream of the content</returns>
        public Stream Open(FileMode open, FileAccess access, FileShare share) {
            return this.fileInfo.Open(open, access, share);
        }

        /// <summary>
        /// Moves to target file.
        /// </summary>
        /// <param name="target">Target file name.</param>
        public void MoveTo(string target) {
            if (this.Directory.ReadOnly) {
                var directory = this.Directory;
                try {
                    directory.ReadOnly = false;
                    this.MoveToPossibleReadOnlyTarget(target);
                } finally {
                    directory.ReadOnly = true;
                }
            } else {
                this.MoveToPossibleReadOnlyTarget(target);
            }
        }

        private void MoveToPossibleReadOnlyTarget(string target) {
            var targetInfo = new FileInfoWrapper(new FileInfo(target));
            if (targetInfo.Directory.Exists && targetInfo.Directory.ReadOnly) {
                try {
                    targetInfo.Directory.ReadOnly = false;
                    this.fileInfo.MoveTo(target);
                } finally {
                    targetInfo.Directory.ReadOnly = true;
                }
            } else {
                this.fileInfo.MoveTo(target);
            }
        }

        /// <summary>
        /// Replaces the contents of a specified destinationFile with the file described by the current IFileInfo object, deleting the original file, and creating a backup of the replaced file.
        /// Also specifies whether to ignore merge errors.
        /// </summary>
        /// <param name="destinationFile">Destination file.</param>
        /// <param name="destinationBackupFileName">Destination backup file name.</param>
        /// <param name="ignoreMetadataErrors"><c>true</c> to ignore merge errors (such as attributes and ACLs) from the replaced file to the replacement file; otherwise <c>false</c>.</param>
        /// <returns>A IFileInfo object that encapsulates information about the file described by the destFileName parameter.</returns>
        public IFileInfo Replace(IFileInfo destinationFile, IFileInfo destinationBackupFileName, bool ignoreMetadataErrors) {
            if (destinationFile.ReadOnly) {
                try {
                    destinationFile.ReadOnly = false;
                    bool readOnly = this.ReadOnly;
                    var result = this.fileInfo.Replace(destinationFile, destinationBackupFileName, ignoreMetadataErrors);
                    result.ReadOnly = readOnly;
                    if (destinationBackupFileName != null) {
                        destinationBackupFileName.ReadOnly = true;
                    }

                    return result;
                } catch(Exception) {
                    destinationFile.ReadOnly = true;
                    throw;
                }
            } else {
                bool readOnly = this.ReadOnly;
                var result = this.fileInfo.Replace(destinationFile, destinationBackupFileName, ignoreMetadataErrors);
                result.ReadOnly = readOnly;
                if (destinationBackupFileName != null) {
                    destinationBackupFileName.ReadOnly = false;
                }

                return result;
            }
        }

        /// <summary>
        /// Deletes the file on the fs.
        /// </summary>
        public void Delete() {
            if (this.Directory.ReadOnly) {
                try {
                    this.Directory.ReadOnly = false;
                    this.DeleteFileIfReadOnly();
                } finally {
                    this.Directory.ReadOnly = true;
                }
            } else {
                this.DeleteFileIfReadOnly();
            }
        }

        private void DeleteFileIfReadOnly() {
            if (this.ReadOnly) {
                try {
                    this.ReadOnly = false;
                    this.fileInfo.Delete();
                } catch (Exception) {
                    this.ReadOnly = true;
                    throw;
                }
            } else {
                this.fileInfo.Delete();
            }
        }
    }
}