using System;

namespace CmisSync.Lib.Storage.FileSystem
{
    public class DateTimeToFileConverter
    {
        public static DateTime Convert(DateTime originalDate, FSType fsType)
        {
            switch(fsType) {
            case FSType.NTFS:
                return limitDateTime(originalDate, new DateTime(1601, 1, 1), new DateTime(5000, 1, 1));
            case FSType.ext2:
                goto case FSType.ext3;
            case FSType.ext3:
                return limitDateTime(originalDate, new DateTime(1901, 12, 15), new DateTime(2038, 1, 18));
            case FSType.ext4:
                return limitDateTime(originalDate, new DateTime(1901, 12, 15), new DateTime(2514, 4, 25));
            case FSType.FAT12:
                goto case FSType.FAT32X;
            case FSType.FAT16:
                goto case FSType.FAT32X;
            case FSType.FAT16B:
                goto case FSType.FAT32X;
            case FSType.FAT16X:
                goto case FSType.FAT32X;
            case FSType.FAT32:
                goto case FSType.FAT32X;
            case FSType.FAT32X:
                return limitDateTime(originalDate, new DateTime(1980, 1, 1), new DateTime(2099, 12, 31));
            case FSType.HFS_Plus:
                return limitDateTime(originalDate, new DateTime(1904, 1, 1), new DateTime(2040, 2, 6));
            case FSType.reiserfs:
                goto case FSType.ext3;
            case FSType.zfs:
                goto case FSType.Unkown;
            case FSType.Unkown:
                return originalDate;
            default:
                goto case FSType.Unkown;
            }
        }

        private static DateTime limitDateTime(DateTime orig, DateTime min, DateTime max) {
            if (orig < min) {
                return min;
            } else if (orig > max) {
                return max;
            } else {
                return orig;
            }
        }
    }
}