//-----------------------------------------------------------------------
// <copyright file="ReadOnlyIgnoringFileSystemInfoDecorator.cs" company="GRAU DATA AG">
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
    /// Read only ignoring file system info decorator hides a given instance and removes read only flags before writing a change and adds it after successful operation.
    /// </summary>
    public abstract class ReadOnlyIgnoringFileSystemInfoDecorator : IFileSystemInfo {
        private IFileSystemInfo fileSystemInfo;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="CmisSync.Lib.Storage.FileSystem.ReadOnlyIgnoringFileSystemInfoDecorator"/> class.
        /// </summary>
        /// <param name="info">Decorated file system info instance.</param>
        protected ReadOnlyIgnoringFileSystemInfoDecorator(IFileSystemInfo info) {
            if (info == null) {
                throw new ArgumentNullException("info");
            }

            this.fileSystemInfo = info;
        }

        /// <summary>
        /// Gets the full name/path.
        /// </summary>
        /// <value>The full name.</value>
        public string FullName {
            get { return this.fileSystemInfo.FullName; }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name {
            get { return this.fileSystemInfo.Name; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="CmisSync.Lib.Storage.FileSystem.IFileSystemInfo"/> is exists.
        /// </summary>
        /// <value><c>true</c> if exists; otherwise, <c>false</c>.</value>
        public bool Exists {
            get { return this.fileSystemInfo.Exists; }
        }

        /// <summary>
        /// Gets or sets the last write time in UTC.
        /// </summary>
        /// <value>The last write time in UTC.</value>
        public DateTime LastWriteTimeUtc {
            get { return this.fileSystemInfo.LastWriteTimeUtc; }
            set { this.DisableAndEnableReadOnlyForOperation(() => this.fileSystemInfo.LastWriteTimeUtc = value); }
        }

        /// <summary>
        /// Gets the creation time in UTC.
        /// </summary>
        /// <value>The creation time in UTC.</value>
        public DateTime CreationTimeUtc {
            get { return this.fileSystemInfo.CreationTimeUtc; }
        }

        /// <summary>
        /// Gets the file attributes.
        /// </summary>
        /// <value>The attributes.</value>
        public FileAttributes Attributes {
            get { return this.fileSystemInfo.Attributes; }
        }

        /// <summary>
        /// Gets or sets the UUID.
        /// </summary>
        /// <value>The UUID.</value>
        public Guid? Uuid {
            get { return this.fileSystemInfo.Uuid; }
            set { this.DisableAndEnableReadOnlyForOperation(() => this.fileSystemInfo.Uuid = value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CmisSync.Lib.Storage.FileSystem.IFileSystemInfo"/>
        /// is read only.
        /// </summary>
        /// <value><c>true</c> if read only; otherwise, <c>false</c>.</value>
        public bool ReadOnly {
            get { return this.fileSystemInfo.ReadOnly; }
            set { this.fileSystemInfo.ReadOnly = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a symlink.
        /// </summary>
        public bool IsSymlink {
            get { return this.fileSystemInfo.IsSymlink; }
        }

        /// <summary>
        /// Refresh the loaded information of this instance.
        /// </summary>
        public void Refresh() {
            this.fileSystemInfo.Refresh();
        }

        /// <summary>
        /// Sets the extended attribute.
        /// </summary>
        /// <param name="key">Attribute name.</param>
        /// <param name="value">Attribute value.</param>
        /// <param name="restoreModificationDate">If set to <c>true</c>, the last modification date of the file will be restored after setting the attribute. If <c>false</c> it could have been changed by the file system.</param>
        public void SetExtendedAttribute(string key, string value, bool restoreModificationDate) {
            this.DisableAndEnableReadOnlyForOperation(() => this.fileSystemInfo.SetExtendedAttribute(key, value, restoreModificationDate));
        }

        /// <summary>
        /// Gets the extended attribute.
        /// </summary>
        /// <returns>The extended attribute value.</returns>
        /// <param name="key">Attribute name.</param>
        public string GetExtendedAttribute(string key) {
            return this.fileSystemInfo.GetExtendedAttribute(key);
        }

        /// <summary>
        /// Determines whether instance is able to save extended attributes.
        /// </summary>
        /// <returns><c>true</c> if this instance is able to save extended attributes; otherwise, <c>false</c>.</returns>
        public bool IsExtendedAttributeAvailable() {
            return this.fileSystemInfo.IsExtendedAttributeAvailable();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Storage.FileSystem.ReadOnlyIgnoringDirectoryInfoDecorator"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Storage.FileSystem.ReadOnlyIgnoringDirectoryInfoDecorator"/>.</returns>
        public override string ToString() {
            return this.fileSystemInfo.ToString();
        }

        /// <summary>
        /// Disables read only before executing action end reenables read only after it.
        /// </summary>
        /// <param name="writeOperation">Write operation.</param>
        protected void DisableAndEnableReadOnlyForOperation(Action writeOperation) {
            this.Refresh();
            if (this.ReadOnly) {
                this.ReadOnly = false;
                try {
                    writeOperation();
                } finally {
                    this.ReadOnly = true;
                }
            } else {
                writeOperation();
            }
        }
    }
}