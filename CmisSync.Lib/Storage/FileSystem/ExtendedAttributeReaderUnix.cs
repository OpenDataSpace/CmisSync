//-----------------------------------------------------------------------
// <copyright file="ExtendedAttributeReaderUnix.cs" company="GRAU DATA AG">
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
    using System.Text;

    #if __MonoCS__
    using Mono.Unix.Native;
    #endif

    /// <summary>
    /// Extended attribute reader for unix.
    /// </summary>
    public class ExtendedAttributeReaderUnix : IExtendedAttributeReader {
        private readonly string prefix = "user.";

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Storage.FileSystem.ExtendedAttributeReaderUnix"/> class.
        /// </summary>
        public ExtendedAttributeReaderUnix() {
#if (__MonoCS__ != true)
            throw new WrongPlatformException();
#endif
        }

        /// <summary>
        /// Gets the extended attribute.
        /// </summary>
        /// <returns>The extended attribute.</returns>
        /// <param name="path">Path to the file or folder.</param>
        /// <param name="key">Key of the extended attribute.</param>
        public string GetExtendedAttribute(string path, string key) {
            path = Path.GetFullPath(path);
            if (!File.Exists(path) && !Directory.Exists(path)) {
                throw new FileNotFoundException(string.Format("{0}: on path \"{1}\"", "No such file or directory", path), path);
            }

#if __MonoCS__
            byte[] value;
            long ret = Syscall.getxattr(path, prefix + key, out value);
#if __COCOA__
            if (ret != 0)
            {
                // On MacOS 93 means no value is found
                if (ret == 93) {
                    return null;
                }
            }
#else
            if (ret == -1) {
                Errno error = Syscall.GetLastError();
                if(error.ToString().Equals("ENODATA")) {
                    return null;
                } else {
                    throw new ExtendedAttributeException(string.Format("{0}: on path \"{1}\"", Syscall.GetLastError().ToString(), path));
                }
            }
#endif
            if (value == null) {
                return null;
            } else {
                return Encoding.UTF8.GetString(value);
            }
#else
            throw new WrongPlatformException();
#endif
        }

        /// <summary>
        /// Sets the extended attribute.
        /// </summary>
        /// <param name="path">Path to the file or folder.</param>
        /// <param name="key">Key of the extended attribute.</param>
        /// <param name="value">Value of the extended attribute.</param>
        /// <param name="restoreModificationDate">If set to <c>true</c> restore last modification date.</param>
        public void SetExtendedAttribute(string path, string key, string value, bool restoreModificationDate = false) {
#if __MonoCS__
            path = Path.GetFullPath(path);
            if (!File.Exists(path) && !Directory.Exists(path)) {
                throw new FileNotFoundException(string.Format("{0}: on path \"{1}\"", "No such file or directory", path), path);
            }

            long ret;
            if (value == null) {
                RemoveExtendedAttribute(path, key);
                return;
            } else {
                ret = Syscall.setxattr(path, prefix + key, Encoding.UTF8.GetBytes(value));
            }

            if (ret != 0) {
                throw new ExtendedAttributeException(string.Format("{0}: on path \"{1}\"", Syscall.GetLastError().ToString(), path));
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
            path = Path.GetFullPath(path);
            if (!File.Exists(path) && !Directory.Exists(path)) {
                throw new FileNotFoundException(string.Format("{0}: on path \"{1}\"", "No such file or directory", path), path);
            }

#if __MonoCS__
            long ret = Syscall.removexattr (path, prefix + key);
            if (ret != 0) {
#if! __COCOA__
                Errno errno = Syscall.GetLastError();
                if (errno != Errno.ENODATA) {
                    throw new ExtendedAttributeException(string.Format("{0}: on path \"{1}\"", errno.ToString(), path));
                }
#endif
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
#if __MonoCS__
            string[] list;
            Syscall.listxattr(path, out list);
            List<string> result = new List<string>();
            foreach (string key in list) {
                if (key.StartsWith(prefix)) {
                    result.Add(key.Substring(prefix.Length));
                }
            }

            return result;
#else
            throw new WrongPlatformException();
#endif
        }

        /// <summary>
        /// Determines whether Extended Attributes are active on the filesystem.
        /// </summary>
        /// <param name="path">Path to be checked</param>
        /// <returns><c>true</c> if this instance is feature available the specified path; otherwise, <c>false</c>.</returns>
        public bool IsFeatureAvailable(string path) {
#if __MonoCS__
            if (!File.Exists(path) && !Directory.Exists(path)) {
                throw new ArgumentException(
                    string.Format(
                        "Given path \"{0}\" does not exists",
                        path));
            }

            byte[] value;
            string key = "test";
            long ret = Syscall.getxattr(path, prefix + key, out value);
            bool retValue = true;
            if (ret != 0) {
#if __COCOA__
                // Feature not supported is errno 102
                if (ret == 102) {
                    retValue = false;
                }
#else
                Errno error = Syscall.GetLastError();
                if(error.ToString().Equals("EOPNOTSUPP")) {
                    retValue = false;
                }
#endif
            }

            return retValue;
#else
            throw new WrongPlatformException();
#endif
        }
    }
}