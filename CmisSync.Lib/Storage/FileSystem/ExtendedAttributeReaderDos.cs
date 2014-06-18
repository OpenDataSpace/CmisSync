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

namespace CmisSync.Lib.Storage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;

    using Microsoft.Win32.SafeHandles;

    public class ExtendedAttributeReaderDos : IExtendedAttributeReader
    {
#if ! __MonoCS__
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFile(
            string fileName,
            FILE_ACCESS_RIGHTS access,
            FileShare share,
            int securityAttributes,
            FileMode creation,
            FILE_FLAGS flags,
            IntPtr templateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DeleteFile(string fileName);

        private enum FILE_ACCESS_RIGHTS : uint
        {
            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000
        }

        private enum FILE_FLAGS : uint
        {
            WriteThrough = 0x80000000,
            Overlapped = 0x40000000,
            NoBuffering = 0x20000000,
            RandomAccess = 0x10000000,
            SequentialScan = 0x8000000,
            DeleteOnClose = 0x4000000,
            BackupSemantics = 0x2000000,
            PosixSemantics = 0x1000000,
            OpenReparsePoint = 0x200000,
            OpenNoRecall = 0x100000
        }
#endif

        public string GetExtendedAttribute(string path, string key)
        {
#if ! __MonoCS__
            if(string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Empty or null key is not allowed");
            }

            IntPtr fileHandle = CreateFile(string.Format("{0}:{1}", path, key), FILE_ACCESS_RIGHTS.GENERIC_READ, FileShare.Read, 0, FileMode.Open, 0, IntPtr.Zero);
            TextReader reader = new StreamReader(new FileStream(new SafeFileHandle(fileHandle, true), FileAccess.Read));

            string result = reader.ReadToEnd();
            reader.Close();
            CloseHandle(fileHandle);

            // int error = Marshal.GetLastWin32Error();
            return result;
#else
            throw new WrongPlatformException();
#endif
        }

        public void SetExtendedAttribute(string path, string key, string value)
        {
#if ! __MonoCS__
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Empty or null key is not allowed");
            }

            IntPtr fileHandle = CreateFile(string.Format("{0}:{1}", path, key), FILE_ACCESS_RIGHTS.GENERIC_WRITE, FileShare.Write, 0, FileMode.Create, 0, IntPtr.Zero);
            TextWriter writer = new StreamWriter(new FileStream(new SafeFileHandle(fileHandle, true), FileAccess.Write));
            writer.Write(value);
            writer.Close();
            CloseHandle(fileHandle);
#else
            throw new WrongPlatformException();
#endif
        }

        public void RemoveExtendedAttribute(string path, string key)
        {
#if ! __MonoCS__
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Empty or null key is not allowed");
            }

            DeleteFile(string.Format("{0}:{1}", path, key));
#else
            throw new WrongPlatformException();
#endif
        }

        public List<string> ListAttributeKeys(string path)
        {
#if ! __MonoCS__
            throw new NotImplementedException();
#else
            throw new WrongPlatformException();
#endif
        }

        public bool IsFeatureAvailable(string path)
        {
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
