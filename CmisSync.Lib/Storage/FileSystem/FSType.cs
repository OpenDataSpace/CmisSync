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

namespace CmisSync.Lib.Storage.FileSystem
{
    using System;

    public enum FSType
    {
        Unkown,
        NTFS,
        FAT12,
        FAT16,
        FAT16B,
        FAT16X,
        FAT32,
        FAT32X,
        HFS_Plus,
        ext2,
        ext3,
        ext4,
        reiserfs,
        btrfs,
        zfs
    }

    public static class FSTypeCreator {
        public static FSType GetType(string type) {
            switch (type) {
            case "NTFS":
                return FSType.NTFS;
            case "FAT32":
                return FSType.FAT32;
            case "ext2":
                return FSType.ext2;
            case "ext3":
                return FSType.ext3;
            case "ext4":
                return FSType.ext4;
            case "hfs":
                return FSType.HFS_Plus;
            default:
                return FSType.Unkown;
            }
        }
    } 
}