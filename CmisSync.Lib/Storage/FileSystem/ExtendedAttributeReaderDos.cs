//-----------------------------------------------------------------------
// <copyright file="ExtendedAttributeReaderDos.cs" company="GRAU DATA AG">
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
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Text.RegularExpressions;

    using Alphaleonis.Win32.Filesystem;

    /// <summary>
    /// Extended attribute reader for Windows.
    /// </summary>
    public class ExtendedAttributeReaderDos : IExtendedAttributeReader {
        /// <summary>
        /// Retrieves the extended attribute.
        /// </summary>
        /// <returns>The attribute value orr <c>null</c> if the attribute does not exist.</returns>
        /// <param name="path">Retrrieves attribute of this path.</param>
        /// <param name="key">Key of the attribute, which should be retrieved.</param>
        public string GetExtendedAttribute(string path, string key) {
#if ! __MonoCS__
            if (string.IsNullOrEmpty(key)) {
                throw new ArgumentException("Empty or null key is not allowed");
            }

            path = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
            if (!File.Exists(path) && !Directory.Exists(path)) {
                throw new System.IO.FileNotFoundException(string.Format("{0}: on path \"{1}\"", "No such file or directory", path), path);
            }

            try {
                return File.ReadAllText(Path.Combine(string.Format("{0}:{1}", path, key)), PathFormat.FullPath);
            } catch (System.IO.FileNotFoundException) {
                // Stream not found.
                return null;
            }
#else
            throw new WrongPlatformException();
#endif
        }

        /// <summary>
        /// Sets the extended attribute.
        /// </summary>
        /// <param name="path">Sets attribute of this path.</param>
        /// <param name="key">Key of the attribute, which should be set.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="restoreModificationDate">If <c>true</c> the modification date is restored after setting extended attribute.</param>
        public void SetExtendedAttribute(string path, string key, string value, bool restoreModificationDate = false) {
            if (restoreModificationDate) {
                this.SetExtendedAttributeAndRestoreLastModificationDate(path, key, value);
            } else {
#if ! __MonoCS__
            if (string.IsNullOrEmpty(key)) {
                throw new ArgumentException("Empty or null key is not allowed");
            }

            path = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
            if (!File.Exists(path) && !Directory.Exists(path)) {
                throw new System.IO.FileNotFoundException(string.Format("{0}: on path \"{1}\"", "No such file or directory", path), path);
            }

            if (value == null) {
                this.RemoveExtendedAttribute(path, key);
            } else {
                try {
                    File.WriteAllText(Path.Combine(string.Format("{0}:{1}", path, key)), value, PathFormat.FullPath);
                } catch (UnauthorizedAccessException ex) {
                    throw new System.IO.IOException(ex.Message, ex);
                }
            }
#else
            throw new WrongPlatformException();
#endif
            }
        }

        /// <summary>
        /// Sets the extended attribute and restore last modification date.
        /// </summary>
        /// <param name="path">Sets attribute of this path.</param>
        /// <param name="key">Key of the attribute, which should be set.</param>
        /// <param name="value">The value to set.</param>
        private void SetExtendedAttributeAndRestoreLastModificationDate(string path, string key, string value) {
            #if ! __MonoCS__
            if (string.IsNullOrEmpty(key)) {
                throw new ArgumentException("Empty or null key is not allowed");
            }

            path = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
            if (!File.Exists(path) && !Directory.Exists(path)) {
                throw new System.IO.FileNotFoundException(string.Format("{0}: on path \"{1}\"", "No such file or directory", path), path);
            }

            DateTime oldDate = File.Exists(path) ? File.GetLastWriteTimeUtc(path) : Directory.GetLastWriteTimeUtc(path);
            if (value == null) {
                this.RemoveExtendedAttribute(path, key);
            } else {
                try {
                    File.WriteAllText(Path.Combine(string.Format("{0}:{1}", path, key)), value, PathFormat.FullPath);
                } catch (UnauthorizedAccessException ex) {
                    throw new System.IO.IOException(ex.Message, ex);
                }
            }

            try {
                if (File.Exists(path)) {
                    File.SetLastWriteTimeUtc(path, oldDate);
                } else {
                    Directory.SetLastWriteTimeUtc(path, oldDate);
                }
            } catch (System.IO.IOException ex) {
                throw new RestoreModificationDateException("Cannot restore last modification date on " + path, ex);
            }
            #else
            throw new WrongPlatformException();
            #endif
        }

        /// <summary>
        /// Removes the extended attribute.
        /// </summary>
        /// <param name="path">Removes attribute from this path.</param>
        /// <param name="key">Key of the attribute, which should be removed.</param>
        public void RemoveExtendedAttribute(string path, string key) {
#if ! __MonoCS__
            if (string.IsNullOrEmpty(key)) {
                throw new ArgumentException("Empty or null key is not allowed");
            }

            path = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
            if (!File.Exists(path) && !Directory.Exists(path)) {
                throw new System.IO.FileNotFoundException(string.Format("{0}: on path \"{1}\"", "No such file or directory", path), path);
            }

            try {
                File.Delete(Path.Combine(string.Format("{0}:{1}", path, key)), true, PathFormat.FullPath);
            } catch (UnauthorizedAccessException ex) {
                throw new System.IO.IOException(ex.Message, ex);
            }
#else
            throw new WrongPlatformException();
#endif
        }

        /// <summary>
        /// Lists the attribute keys.
        /// </summary>
        /// <returns>The attribute keys.</returns>
        /// <param name="path">Path which should be read.</param>
        public List<string> ListAttributeKeys(string path) {
#if ! __MonoCS__
            if (string.IsNullOrEmpty(path)) {
                throw new ArgumentNullException("path");
            }

            path = Path.GetFullPath(path);
            var result = new List<string>();
            foreach (var stream in File.EnumerateAlternateDataStreams(path)) {
                var key = stream.StreamName;
                if (!string.IsNullOrEmpty(key)) {
                    result.Add(key);
                }
            }

            return result;
#else
            throw new WrongPlatformException();
#endif
        }

        /// <summary>
        /// Determines whether Extended Attributes are available on the filesystem.
        /// </summary>
        /// <param name="path">Path to be checked</param>
        /// <returns><c>true</c> if this feature is available for the specified path; <c>false</c> otherwise.</returns>
        public bool IsFeatureAvailable(string path) {
#if ! __MonoCS__
            string fullPath = new FileInfo(path).FullName;
            DriveInfo info = new DriveInfo(Path.GetPathRoot(fullPath));
            return info.DriveFormat.Equals("NTFS", StringComparison.OrdinalIgnoreCase);
#else
            throw new WrongPlatformException();
#endif
        }
    }
}