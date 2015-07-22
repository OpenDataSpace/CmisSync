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

namespace CmisSync.Lib.Storage.FileSystem {
    using System;
    using System.IO;
#if !__MonoCS__
    using System.Security.AccessControl;
    using System.Security.Principal;
#endif
    using System.Threading;

#if __MonoCS__
    using Mono.Unix;
#endif

    /// <summary>
    /// Wrapper for DirectoryInfo
    /// </summary>
    public abstract class FileSystemInfoWrapper : IFileSystemInfo {
        private static readonly string ExtendedAttributeKey = "DSS-UUID";
        private static IExtendedAttributeReader reader = null;
#if !__MonoCS__
        private static SecurityIdentifier actualUser = null;
#endif
        private FileSystemInfo original;
        private FSType? fsType = null;

        static FileSystemInfoWrapper() {
            switch (Environment.OSVersion.Platform) {
            case PlatformID.MacOSX:
                goto case PlatformID.Unix;
            case PlatformID.Unix:
                reader = new ExtendedAttributeReaderUnix();
                break;
            case PlatformID.Win32NT:
                reader = new ExtendedAttributeReaderDos();
#if !__MonoCS__
                actualUser = new NTAccount(Environment.UserName).Translate(typeof(SecurityIdentifier)) as SecurityIdentifier;
#endif
                break;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Storage.FileSystem.FileSystemInfoWrapper"/> class.
        /// </summary>
        /// <param name="original">original internal instance.</param>
        protected FileSystemInfoWrapper(FileSystemInfo original) {
            this.original = original;
        }

        /// <summary>
        /// Gets or sets the last write time in UTC.
        /// </summary>
        /// <value>The last write time in UTC.</value>
        public DateTime LastWriteTimeUtc {
            get {
                return this.original.LastWriteTimeUtc;
            }

            set {
                var date = value.IsPerhapsOutOfValidFileSystemRange() ? value.Convert(this.FSType) : value;
                this.original.LastWriteTimeUtc = date;
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
        /// Gets or sets a value indicating whether this
        /// <see cref="CmisSync.Lib.Storage.FileSystem.FileSystemInfoWrapper"/> read only.
        /// </summary>
        /// <value><c>true</c> if read only; otherwise, <c>false</c>.</value>
        public bool ReadOnly {
            get {
                return (this.original.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
            }

            set {
                if (value != this.ReadOnly) {
                    if (value) {
                        this.original.Attributes |= FileAttributes.ReadOnly;
#if !__MonoCS__
                        this.AddReadOnlyAclsToOriginal();
#endif
                    } else {
                        this.original.Attributes &= ~FileAttributes.ReadOnly;
#if !__MonoCS__
                        this.RemoveReadOnlyAclsFromOriginal();
#endif
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a symlink.
        /// </summary>
        public bool IsSymlink {
            get {
                return (this.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
            }
        }

        /// <summary>
        /// Gets or sets the UUID by reading or writing extended attribute entries.
        /// </summary>
        /// <value>The UUID.</value>
        public Guid? Uuid {
            get {
                int retries = 100;
                while (retries > 0) {
                    try {
                        Guid uuid;
                        string storedValue = this.GetExtendedAttribute(ExtendedAttributeKey);
                        if (storedValue != null && Guid.TryParse(storedValue, out uuid)) {
                            return uuid;
                        } else {
                            return null;
                        }
                    } catch (ExtendedAttributeException) {
                        Thread.Sleep(50);
                        retries--;
                        if (retries <= 0) {
                            throw;
                        }
                    }
                }

                return null;
            }

            set {
                int retries = 100;
                while (retries > 0) {
                    try {
                        this.SetExtendedAttribute(ExtendedAttributeKey, value == null ? null : value.ToString(), true);
                        break;
                    } catch(ExtendedAttributeException) {
                        Thread.Sleep(50);
                        retries--;
                        if (retries <= 0) {
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Refresh the loaded information of this instance.
        /// </summary>
        public void Refresh() {
            this.original.Refresh();
        }

        /// <summary>
        /// Gets the extended attribute.
        /// </summary>
        /// <returns>The extended attribute value.</returns>
        /// <param name="key">Attribute name.</param>
        public string GetExtendedAttribute(string key) {
            if (reader != null) {
                return reader.GetExtendedAttribute(this.original.FullName, key);
            } else {
                throw new ExtendedAttributeException("Feature is not supported");
            }
        }

        /// <summary>
        /// Sets the extended attribute.
        /// </summary>
        /// <param name="key">Attribute name.</param>
        /// <param name="value">Attribute value.</param>
        /// <param name="restoreModificationDate">Restore the modification date of the file after setting ea.</param>
        public void SetExtendedAttribute(string key, string value, bool restoreModificationDate = false) {
            if (reader != null) {
                reader.SetExtendedAttribute(this.original.FullName, key, value, restoreModificationDate);
            } else {
                throw new ExtendedAttributeException("Feature is not supported");
            }
        }

        /// <summary>
        /// Determines whether instance is able to save extended attributes.
        /// </summary>
        /// <returns><c>true</c> if extended attributes are available, otherwise<c>false</c></returns>
        public bool IsExtendedAttributeAvailable() {
            if (reader != null) {
                return reader.IsFeatureAvailable(this.original.FullName);
            } else {
                return false;
            }
        }

        /// <summary>
        /// Gets the type of the FS.
        /// </summary>
        /// <value>The type of the FS.</value>
        public FSType FSType {
            get {
                if (this.fsType == null) {
#if __MonoCS__
                    this.fsType = FSTypeCreator.GetType(new UnixDriveInfo(Path.GetPathRoot(this.original.FullName)).DriveFormat);
#else
                    try {
                        this.fsType = FSTypeCreator.GetType(new DriveInfo(Path.GetPathRoot(this.original.FullName)).DriveFormat);
                    } catch (ArgumentException) {
                        this.fsType = FSType.Unknown;
                    }
#endif
                }

                return this.fsType.GetValueOrDefault(FSType.Unknown);
            }
        }

#if !__MonoCS__
        private void AddReadOnlyAclsToOriginal() {
            if (this.original is DirectoryInfo) {
                var dir = this.original as DirectoryInfo;
                var acls = dir.GetAccessControl();
                acls.AddAccessRule(new FileSystemAccessRule(actualUser, FileSystemRights.WriteData, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Deny));
                acls.AddAccessRule(new FileSystemAccessRule(actualUser, FileSystemRights.CreateDirectories, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Deny));
                acls.AddAccessRule(new FileSystemAccessRule(actualUser, FileSystemRights.CreateFiles, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Deny));
                acls.AddAccessRule(new FileSystemAccessRule(actualUser, FileSystemRights.Delete, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Deny));
                dir.SetAccessControl(acls);
            } else if (this.original is FileInfo) {
                var file = this.original as FileInfo;
                var acls = file.GetAccessControl();
                acls.AddAccessRule(new FileSystemAccessRule(actualUser, FileSystemRights.WriteData, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Deny));
                acls.AddAccessRule(new FileSystemAccessRule(actualUser, FileSystemRights.AppendData, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Deny));
                acls.AddAccessRule(new FileSystemAccessRule(actualUser, FileSystemRights.Delete, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Deny));
                file.SetAccessControl(acls);
            }
        }

        private void RemoveReadOnlyAclsFromOriginal() {
            if (this.original is DirectoryInfo) {
                var dir = this.original as DirectoryInfo;
                var acls = dir.GetAccessControl();
                acls.RemoveAccessRule(new FileSystemAccessRule(actualUser, FileSystemRights.WriteData, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Deny));
                acls.RemoveAccessRule(new FileSystemAccessRule(actualUser, FileSystemRights.CreateDirectories, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Deny));
                acls.RemoveAccessRule(new FileSystemAccessRule(actualUser, FileSystemRights.CreateFiles, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Deny));
                acls.RemoveAccessRule(new FileSystemAccessRule(actualUser, FileSystemRights.Delete, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Deny));
                dir.SetAccessControl(acls);
            } else if (this.original is FileInfo) {
                var file = this.original as FileInfo;
                var acls = file.GetAccessControl();
                acls.RemoveAccessRule(new FileSystemAccessRule(actualUser, FileSystemRights.WriteData, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Deny));
                acls.RemoveAccessRule(new FileSystemAccessRule(actualUser, FileSystemRights.AppendData, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Deny));
                acls.RemoveAccessRule(new FileSystemAccessRule(actualUser, FileSystemRights.Delete, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Deny));
                file.SetAccessControl(acls);
            }
        }
#endif
    }
}