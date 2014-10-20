using System;

namespace CmisSync.Lib.Storage.FileSystem
{
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
            default:
                return FSType.Unkown;
            }
        }
    } 
}