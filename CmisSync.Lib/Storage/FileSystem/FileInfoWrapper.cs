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

namespace CmisSync.Lib.Storage.FileSystem {
    using System;
    using System.Collections.Generic;
    using System.IO;
#if !__MONO_CS__
    using System.Runtime.InteropServices;
#endif

    /// <summary>
    /// Wrapper for FileInfo
    /// </summary>
    public class FileInfoWrapper : FileSystemInfoWrapper, IFileInfo {
        private FileInfo original;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Storage.FileSystem.FileInfoWrapper"/> class.
        /// </summary>
        /// <param name="fileInfo">File info.</param>
        public FileInfoWrapper(FileInfo fileInfo) : base(fileInfo) {
            this.original = fileInfo;
        }

        /// <summary>
        /// Gets the parent directory.
        /// </summary>
        /// <value>The parent directory.</value>
        public IDirectoryInfo Directory {
            get {
                return new DirectoryInfoWrapper(this.original.Directory);
            }
        }

        /// <summary>
        /// Gets the file content length.
        /// </summary>
        /// <value>The length.</value>
        public long Length {
            get {
                return this.original.Length;
            }
        }

        /// <summary>
        /// Open the specified file.
        /// </summary>
        /// <param name="mode">Open mode.</param>
        /// <returns>Stream of the content.</returns>
        public Stream Open(FileMode mode) {
            return this.original.Open(mode);
        }

        /// <summary>
        /// Open the specified file with the open and access mode.
        /// </summary>
        /// <param name="mode">Open mode.</param>
        /// <param name="access">Access Mode.</param>
        /// <returns>Stream of the content</returns>
        public Stream Open(FileMode mode, FileAccess access) {
            return this.original.Open(mode, access);
        }

        /// <summary>
        /// Open the specified file with given open, access and share mode.
        /// </summary>
        /// <param name="mode">Open mode.</param>
        /// <param name="access">Access mode.</param>
        /// <param name="share">Share mode.</param>
        /// <returns>Stream of the content</returns>
        public Stream Open(FileMode mode, FileAccess access, FileShare share) {
            return this.original.Open(mode, access, share);
        }

        /// <summary>
        /// Moves to target file.
        /// </summary>
        /// <param name="target">Target file name.</param>
        public void MoveTo(string target) {
            this.original.MoveTo(target);
        }

        /// <summary>
        /// Deletes the file on the fs.
        /// </summary>
        public void Delete() {
            this.original.Delete();
        }

        /// <summary>
        /// Replaces the contents of a specified destinationFile with the file described by the current IFileInfo
        /// object, deleting the original file, and creating a backup of the replaced file.
        /// Also specifies whether to ignore merge errors.
        /// </summary>
        /// <param name="destinationFile">Destination file.</param>
        /// <param name="destinationBackupFileName">Destination backup file name.</param>
        /// <param name="ignoreMetadataErrors"><c>true</c> to ignore merge errors (such as attributes and ACLs) from the replaced file to the replacement file; otherwise <c>false</c>.</param>
        /// <returns>A IFileInfo object that encapsulates information about the file described by the destFileName parameter.</returns>
        public IFileInfo Replace(IFileInfo destinationFile, IFileInfo destinationBackupFileName, bool ignoreMetadataErrors) {
            if (destinationFile == null) {
                throw new ArgumentNullException("destinationFile");
            }
#if __MonoCS__
            var reader = new ExtendedAttributeReaderUnix();
            var oldSourceEAs = new Dictionary<string, string>();
            var oldTargetEAs = new Dictionary<string, string>();
            if (reader.IsFeatureAvailable(this.FullName)) {
                foreach (var key in reader.ListAttributeKeys(this.FullName)) {
                    oldSourceEAs.Add(key, this.GetExtendedAttribute(key));
                }

                foreach (var key in reader.ListAttributeKeys(destinationFile.FullName)) {
                    oldTargetEAs.Add(key, destinationFile.GetExtendedAttribute(key));
                }
            }
#else
            try {
#endif
                var result = new FileInfoWrapper(this.original.Replace(destinationFile.FullName, destinationBackupFileName != null ? destinationBackupFileName.FullName : null, ignoreMetadataErrors));
#if __MonoCS__
            foreach (var entry in oldSourceEAs) {
                result.SetExtendedAttribute(entry.Key, entry.Value, true);
            }

            foreach (var entry in oldTargetEAs) {
                destinationBackupFileName.SetExtendedAttribute(entry.Key, entry.Value, true);
            }

            return result;
#else
                return result;
            } catch (IOException ex) {
                int error = Marshal.GetHRForException(ex) & 0xffff;
                if (error == 1176) {
                    string newName = destinationFile.FullName + Guid.NewGuid().ToString() + ".sync";
                    IFileInfo newResult = null;
                    try {
                        var copy = this.original.CopyTo(newName, true);
                        newResult = new FileInfoWrapper(copy.Replace(destinationFile.FullName, destinationBackupFileName.FullName, ignoreMetadataErrors));
                        this.Delete();
                        return newResult;
                    } catch (Exception) {
                    } finally {
                        if (File.Exists(newName)) {
                            File.Delete(newName);
                        }
                    }

                    throw;
                } else {
                    throw;
                }
            }
#endif
        }
    }
}
