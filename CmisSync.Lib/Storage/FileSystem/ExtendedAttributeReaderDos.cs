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

namespace CmisSync.Lib.Storage.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text.RegularExpressions;
    using System.Text;

    using Microsoft.Win32.SafeHandles;

    /// <summary>
    /// Extended attribute reader for Windows.
    /// </summary>
    public class ExtendedAttributeReaderDos : IExtendedAttributeReader
    {
#if ! __MonoCS__
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(
            string name,
            FileAccess access,
            FileShare share,
            IntPtr security,
            FileMode mode,
            FILE_FLAGS flags,
            IntPtr templateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DeleteFile(string fileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        private static extern int FormatMessage(
                        uint dwFlags,
                        IntPtr lpSource,
                        int dwMessageId,
                        uint dwLanguageId,
                        StringBuilder lpBuffer,
                        int nSize,
                        IntPtr vaListArguments);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool BackupRead(SafeFileHandle hFile, IntPtr lpBuffer,
            uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead,
            [MarshalAs(UnmanagedType.Bool)] bool bAbort,
            [MarshalAs(UnmanagedType.Bool)] bool bProcessSecurity,
            ref IntPtr lpContext);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool BackupSeek(SafeFileHandle hFile,
            uint dwLowBytesToSeek, uint dwHighBytesToSeek,
            out uint lpdwLowByteSeeked, out uint lpdwHighByteSeeked,
            ref IntPtr lpContext);

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

        private const int ErrorFileNotFound = 2;

        [StructLayout(LayoutKind.Sequential, Pack=1)]
        private struct Win32StreamID
        {
            public StreamType dwStreamId;
            public int dwStreamAttributes;
            public long Size;
            public int dwStreamNameSize;
        }

        private enum StreamType
        {
            Data = 1,
            ExternalData = 2,
            SecurityData = 3,
            AlternateData = 4,
            Link = 5,
            PropertyData = 6,
            ObjectID = 7,
            ReparseData = 8,
            SparseDock = 9
        }

        private struct StreamInfo
        {
            public StreamInfo(string name, StreamType type, long size)
            {
                Name = name;
                Type = type;
                Size = size;
            }
            public readonly string Name;
            public readonly StreamType Type;
            public readonly long Size;
        }

        private static string GetLastErrorMessage()
        {
            int errorCode = Marshal.GetLastWin32Error();
            var lpBuffer = new StringBuilder(0x200);
            if (0 != FormatMessage(0x3200, IntPtr.Zero, errorCode, 1033, lpBuffer, lpBuffer.Capacity, IntPtr.Zero))
            {
                return lpBuffer.ToString();
            }
            return string.Format("0x{0:X8}", errorCode);
        }

        private static SafeFileHandle CreateFileHandle(string path, FileAccess access, FileMode mode, FileShare share)
        {
            // FILE_FLAGS.BackupSemantics is required in order to support directories.
            // Otherwise we get an Access denied, if the  path points to a directory.
            SafeFileHandle handle = CreateFile(path, access, share, IntPtr.Zero, mode, FILE_FLAGS.BackupSemantics, IntPtr.Zero);
            if (handle.IsInvalid)
            {
                throw new ExtendedAttributeException(string.Format("{0}: on path \"{1}\"", GetLastErrorMessage(), path));
            }
            return handle;
        }

        private static FileStream CreateFileStream(string path, FileAccess access, FileMode mode, FileShare share)
        {
            return new FileStream(CreateFileHandle(path, access, mode, share), access);
        }

        private static IEnumerable<string> GetKeys(string path)
        {
            Regex rx = new Regex(@":([^:]+):\$DATA");
            using (SafeFileHandle fh = CreateFileHandle(path, FileAccess.Read, FileMode.Open, FileShare.Read))
            {
                List<StreamInfo> streams = new List<StreamInfo>(GetStreams(fh));

                foreach (StreamInfo stream in streams)
                {
                    if (stream.Type == StreamType.AlternateData ||
                            stream.Type == StreamType.Data)
                    {
                        yield return rx.Replace(stream.Name, "$1");
                    }
                }
            }
        }

        private static IEnumerable<StreamInfo> GetStreams(SafeFileHandle fh)
        {
            const int bufferSize = 4096;
            IntPtr context = IntPtr.Zero;
            IntPtr buffer = Marshal.AllocHGlobal(bufferSize);
            try
            {
                while (true)
                {
                    uint numRead;
                    if (!BackupRead(fh, buffer, (uint)Marshal.SizeOf(typeof(Win32StreamID)),
                                out numRead, false, true, ref context))
                    {
                        throw new IOException("Cannot read stream info");
                    }
                    if (numRead > 0)
                    {
                        Win32StreamID streamID = (Win32StreamID)Marshal.PtrToStructure(buffer, typeof(Win32StreamID));
                        string name = null;
                        if (streamID.dwStreamNameSize > 0)
                        {
                            if (!BackupRead(fh, buffer, (uint)Math.Min(bufferSize, streamID.dwStreamNameSize),
                                        out numRead, false, true, ref context))
                            {
                                throw new IOException("Cannot read stream info");
                            }
                            name = Marshal.PtrToStringUni(buffer, (int)numRead / 2);
                        }

                        if (!string.IsNullOrEmpty(name))
                        {
                            yield return new StreamInfo(name, streamID.dwStreamId, streamID.Size);
                        }

                        if (streamID.Size > 0)
                        {
                            uint lo, hi;
                            BackupSeek(fh, uint.MaxValue, int.MaxValue, out lo, out hi, ref context);
                        }
                    }
                    else break;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
                uint numRead;
                if (!BackupRead(fh, IntPtr.Zero, 0, out numRead, true, false, ref context))
                {
                    throw new IOException("Cannot read stream info");
                }
            }
        }
#endif

        /// <summary>
        /// Retrieves the extended attribute.
        /// </summary>
        /// <returns>The attribute value orr <c>null</c> if the attribute does not exist.</returns>
        /// <param name="path">Retrrieves attribute of this path.</param>
        /// <param name="key">Key of the attribute, which should be retrieved.</param>
        public string GetExtendedAttribute(string path, string key)
        {
#if ! __MonoCS__
            if(string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Empty or null key is not allowed");
            }
            path = Path.GetFullPath(path);
            path = path.TrimEnd(Path.DirectorySeparatorChar);
            if (!File.Exists(path) && !Directory.Exists(path)) {
                throw new FileNotFoundException(string.Format("{0}: on path \"{1}\"", "No such file or directory", path), path);
            }
            try {
                using (FileStream stream = CreateFileStream(string.Format("{0}:{1}", path, key), FileAccess.Read, FileMode.Open, FileShare.Read))
                {
                    TextReader reader = new StreamReader(stream);
                    string result = reader.ReadToEnd();
                    reader.Close();
                    return result;
                }
            } catch (ExtendedAttributeException e) {
                if (ErrorFileNotFound == Marshal.GetLastWin32Error()) {
                    // Stream not found.
                    return null;
                }
                throw e;
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
        public void SetExtendedAttribute(string path, string key, string value, bool restoreLastModificationDate = false)
        {
            if (restoreLastModificationDate) {
                SetExtendedAttributeAndRestoreLastModificationDate(path, key, value);
            } else {

#if ! __MonoCS__
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Empty or null key is not allowed");
            }

            path = Path.GetFullPath(path);
            path = path.TrimEnd(Path.DirectorySeparatorChar);
            if (!File.Exists(path) && !Directory.Exists(path)) {
                throw new FileNotFoundException(string.Format("{0}: on path \"{1}\"", "No such file or directory", path), path);
            }

            if (value == null)
            {
                RemoveExtendedAttribute(path, key);
            }
            else
            {
                using (FileStream stream = CreateFileStream(string.Format("{0}:{1}", path, key), FileAccess.Write, FileMode.Create, FileShare.Write))
                {
                    TextWriter writer = new StreamWriter(stream);
                    writer.Write(value);
                    writer.Close();
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
        private void SetExtendedAttributeAndRestoreLastModificationDate(string path, string key, string value)
        {
            #if ! __MonoCS__
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Empty or null key is not allowed");
            }

            path = Path.GetFullPath(path);
            path = path.TrimEnd(Path.DirectorySeparatorChar);
            if (!File.Exists(path) && !Directory.Exists(path)) {
                throw new FileNotFoundException(string.Format("{0}: on path \"{1}\"", "No such file or directory", path), path);
            }
            DateTime oldDate = File.Exists(path) ? File.GetLastWriteTimeUtc(path) : Directory.GetLastWriteTimeUtc(path);
            if (value == null)
            {
                RemoveExtendedAttribute(path, key);
            }
            else
            {
                using (FileStream stream = CreateFileStream(string.Format("{0}:{1}", path, key), FileAccess.Write, FileMode.Create, FileShare.Write))
                {
                    TextWriter writer = new StreamWriter(stream);
                    writer.Write(value);
                    writer.Close();
                }
            }

            try {
                if (File.Exists(path)) {
                    File.SetLastWriteTimeUtc(path, oldDate);
                } else {
                    Directory.SetLastWriteTimeUtc(path, oldDate);
                }
            } catch (IOException ex) {
                throw new ExtendedAttributeException("Cannot restore last modification date on " + path, ex) {
                    ExceptionOnUpdatingModificationDate = true
                };
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
        public void RemoveExtendedAttribute(string path, string key)
        {
#if ! __MonoCS__
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Empty or null key is not allowed");
            }
            path = Path.GetFullPath(path);
            if (!File.Exists(path) && !Directory.Exists(path)) {
                throw new FileNotFoundException(string.Format("{0}: on path \"{1}\"", "No such file or directory", path), path);
            }
            new FileIOPermission(FileIOPermissionAccess.Write, path).Demand();
            if (!DeleteFile(string.Format("{0}:{1}:{2}", path, key, "$DATA"))) {
                if (ErrorFileNotFound != Marshal.GetLastWin32Error()) {
                    throw new ExtendedAttributeException(string.Format("{0}: on path \"{1}\"", GetLastErrorMessage(),
                                string.Format("{0}:{1}:{2}", path, key, "$DATA")));
                }
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
        public List<string> ListAttributeKeys(string path)
        {
#if ! __MonoCS__
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }
            path = Path.GetFullPath(path);
            // Explicitely request read permission in order to support directories.
            new FileIOPermission(FileIOPermissionAccess.Read, path).Demand();
            return new List<string>(GetKeys(path));
#else
            throw new WrongPlatformException();
#endif
        }

        /// <summary>
        /// Determines whether Extended Attributes are available on the filesystem.
        /// </summary>
        /// <param name="path">Path to be checked</param>
        /// <returns><c>true</c> if this feature is available for the specified path; <c>false</c> otherwise.</returns>
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
