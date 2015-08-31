//-----------------------------------------------------------------------
// <copyright file="DateTimeToFileConverter.cs" company="GRAU DATA AG">
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
    /// Date time to file converter.
    /// </summary>
    public static class DateTimeToFileConverter {
#if __MonoCS__
        private static readonly DateTime MinDate = new DateTime(1972, 1, 1);
        private static readonly DateTime MaxDate = new DateTime(2038, 1, 18);
#else
        private static readonly DateTime MinDate = new DateTime(1601, 1, 1);
        private static readonly DateTime MaxDate = new DateTime(5000, 1, 1);
#endif

        /// <summary>
        /// Determines if the given date is perhaps out of valid file system range for modification dates.
        /// </summary>
        /// <returns><c>true</c> if is perhaps out of valid file system range for modification dates; otherwise, <c>false</c>.</returns>
        /// <param name="date">Possible modification date.</param>
        public static bool IsPerhapsOutOfValidFileSystemRange(this DateTime date) {
            return date < MinDate || date > MaxDate;
        }

        /// <summary>
        /// Convert the specified originalDate based on fsType to the documented limits of the fs type.
        /// </summary>
        /// <param name="originalDate">Original date.</param>
        /// <param name="fsType">File system type.</param>
        /// <returns>Fitting datetime</returns>
        public static DateTime Convert(this DateTime originalDate, FSType fsType) {
#if __MonoCS__
            // https://bugzilla.xamarin.com/show_bug.cgi?id=23933
            originalDate = originalDate < new DateTime(1972, 1, 1) ? new DateTime(1972, 1, 1) : originalDate;
#endif
            switch(fsType) {
            case FSType.Ntfs:
                return LimitDateTime(originalDate, new DateTime(1601, 1, 1), new DateTime(5000, 1, 1));
            case FSType.ext2:
                goto case FSType.ext3;
            case FSType.ext3:
                return LimitDateTime(originalDate, new DateTime(1901, 12, 15), new DateTime(2038, 1, 18));
            case FSType.ext4:
                return LimitDateTime(originalDate, new DateTime(1901, 12, 15), new DateTime(2514, 4, 25));
            case FSType.Fat12:
                goto case FSType.Fat32X;
            case FSType.Fat16:
                goto case FSType.Fat32X;
            case FSType.Fat16B:
                goto case FSType.Fat32X;
            case FSType.Fat16X:
                goto case FSType.Fat32X;
            case FSType.Fat32:
                goto case FSType.Fat32X;
            case FSType.Fat32X:
                return LimitDateTime(originalDate, new DateTime(1980, 1, 1), new DateTime(2099, 12, 31));
            case FSType.HfsPlus:
                return LimitDateTime(originalDate, new DateTime(1904, 1, 1), new DateTime(2040, 2, 6));
            case FSType.reiserfs:
                goto case FSType.ext3;
            case FSType.zfs:
                goto case FSType.Unknown;
            case FSType.Unknown:
                return originalDate;
            default:
                goto case FSType.Unknown;
            }
        }

        private static DateTime LimitDateTime(DateTime orig, DateTime min, DateTime max) {
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