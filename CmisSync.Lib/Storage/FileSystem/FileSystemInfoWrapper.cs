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

namespace CmisSync.Lib.Storage.FileSystem
{
    using System;
    using System.IO;

    /// <summary>
    /// Wrapper for DirectoryInfo
    /// </summary>
    public abstract class FileSystemInfoWrapper : IFileSystemInfo
    {
        private static IExtendedAttributeReader reader = null;

        private FileSystemInfo original;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Storage.FileSystem.FileSystemInfoWrapper"/> class.
        /// </summary>
        /// <param name="original">original internal instance.</param>
        protected FileSystemInfoWrapper(FileSystemInfo original)
        {
            this.original = original;
        }

        /// <summary>
        /// Gets or sets the last write time in UTC.
        /// </summary>
        /// <value>The last write time in UTC.</value>
        public DateTime LastWriteTimeUtc {
            get
            {
                return this.original.LastWriteTimeUtc;
            }

            set
            {
                this.original.LastWriteTimeUtc = value;
            }
        }

        /// <summary>
        /// Gets the creation time in UTC.
        /// </summary>
        /// <value>The creation time in UTC.</value>
        public DateTime CreationTimeUtc {
            get {
                return this.original.CreationTimeUtc;
            }
        }

        /// <summary>
        /// Gets the full name/path.
        /// </summary>
        /// <value>The full name.</value>
        public string FullName {
            get { return this.original.FullName; } 
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name {
            get { return this.original.Name; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="CmisSync.Lib.Storage.FileSystem.FileSystemInfoWrapper"/> is exists.
        /// </summary>
        /// <value><c>true</c> if exists; otherwise, <c>false</c>.</value>
        public bool Exists {
            get { return this.original.Exists; }
        }

        /// <summary>
        /// Gets the file attributes.
        /// </summary>
        /// <value>The attributes.</value>
        public FileAttributes Attributes {
            get { return this.original.Attributes; }
        }

        /// <summary>
        /// Refresh the loaded information of this instance.
        /// </summary>
        public void Refresh()
        {
            this.original.Refresh();
        }

        /// <summary>
        /// Gets the extended attribute.
        /// </summary>
        /// <returns>The extended attribute value.</returns>
        /// <param name="key">Attribute name.</param>
        public string GetExtendedAttribute(string key)
        {
            if(reader != null)
            {
                return reader.GetExtendedAttribute(this.original.FullName, key);
            }
            else
            {
                throw new ExtendedAttributeException("Feature is not supported");
            }
        }

        /// <summary>
        /// Sets the extended attribute.
        /// </summary>
        /// <param name="key">Attribute name.</param>
        /// <param name="value">Attribute value.</param>
        /// <param name="restoreModificationDate">Restore the modification date of the file after setting ea.</param>
        public void SetExtendedAttribute(string key, string value, bool restoreModificationDate = false)
        {
            if(reader != null)
            {
                reader.SetExtendedAttribute(this.original.FullName, key, value, restoreModificationDate);
            }
            else
            {
                throw new ExtendedAttributeException("Feature is not supported");
            }
        }

        /// <summary>
        /// Determines whether instance is able to save extended attributes.
        /// </summary>
        /// <returns><c>true</c> if extended attributes are available, otherwise<c>false</c></returns>
        public bool IsExtendedAttributeAvailable()
        {
            if (reader != null) {
                return reader.IsFeatureAvailable(this.original.FullName);
            }
            else
            {
                return false;
            }
        }
    }
}
