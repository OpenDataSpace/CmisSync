//-----------------------------------------------------------------------
// <copyright file="FSType.cs" company="GRAU DATA AG">
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
    /// File system type.
    /// </summary>
    public enum FSType {
        /// <summary>
        /// Unknown file system.
        /// </summary>
        Unknown,

        /// <summary>
        /// NTFS file system.
        /// </summary>
        Ntfs,

        /// <summary>
        /// FAT12 file system.
        /// </summary>
        Fat12,

        /// <summary>
        /// FAT16 file system.
        /// </summary>
        Fat16,

        /// <summary>
        /// FAT16B file system.
        /// </summary>
        Fat16B,

        /// <summary>
        /// FAT16X file system.
        /// </summary>
        Fat16X,

        /// <summary>
        /// FAT32 file system.
        /// </summary>
        Fat32,

        /// <summary>
        /// FAT32X file system.
        /// </summary>
        Fat32X,

        /// <summary>
        /// HFS+ file system.
        /// </summary>
        HfsPlus,

        /// <summary>
        /// ext2 file system.
        /// </summary>
        ext2,

        /// <summary>
        /// ext3 file system.
        /// </summary>
        ext3,

        /// <summary>
        /// ext4 file system.
        /// </summary>
        ext4,

        /// <summary>
        /// reiserfs file system.
        /// </summary>
        reiserfs,

        /// <summary>
        /// btrfs file system.
        /// </summary>
        btrfs,

        /// <summary>
        /// zfs file system.
        /// </summary>
        zfs
    }

    /// <summary>
    /// File system type creator.
    /// </summary>
    public static class FSTypeCreator {
        /// <summary>
        /// Gets the file system type based on the given type string.
        /// </summary>
        /// <returns>The type string.</returns>
        /// <param name="type">File system type.</param>
        public static FSType GetType(string type) {
            switch (type) {
            case "NTFS":
                return FSType.Ntfs;
            case "FAT32":
                return FSType.Fat32;
            case "ext2":
                return FSType.ext2;
            case "ext3":
                return FSType.ext3;
            case "ext4":
                return FSType.ext4;
            case "hfs":
                return FSType.HfsPlus;
            default:
                return FSType.Unknown;
            }
        }
    } 
}