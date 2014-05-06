//-----------------------------------------------------------------------
// <copyright file="IFileSystemInfo.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Storage
{
    using System;
    using System.IO;

    /// <summary>
    /// Interface to enable mocking of FileSystemInfo
    /// </summary>
    public interface IFileSystemInfo
    {
        /// <summary>
        /// Gets the full name/path.
        /// </summary>
        /// <value>The full name.</value>
        string FullName { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="CmisSync.Lib.Storage.IFileSystemInfo"/> is exists.
        /// </summary>
        /// <value><c>true</c> if exists; otherwise, <c>false</c>.</value>
        bool Exists { get; }

        /// <summary>
        /// Gets or sets the last write time in UTC.
        /// </summary>
        /// <value>The last write time in UTC.</value>
        DateTime LastWriteTimeUtc { get; set; }

        /// <summary>
        /// Gets the file attributes.
        /// </summary>
        /// <value>The attributes.</value>
        FileAttributes Attributes { get; }

        /// <summary>
        /// Refresh the loaded information of this instance.
        /// </summary>
        void Refresh();

        /// <summary>
        /// Sets the extended attribute.
        /// </summary>
        /// <param name="key">Attribute name.</param>
        /// <param name="value">Attribute value.</param>
        void SetExtendedAttribute(string key, string value);

        /// <summary>
        /// Gets the extended attribute.
        /// </summary>
        /// <returns>The extended attribute value.</returns>
        /// <param name="key">Attribute name.</param>
        string GetExtendedAttribute(string key);

        /// <summary>
        /// Determines whether instance is able to save extended attributes.
        /// </summary>
        /// <returns><c>true</c> if this instance is able to save extended attributes; otherwise, <c>false</c>.</returns>
        bool IsExtendedAttributeAvailable();
    }
}
