//-----------------------------------------------------------------------
// <copyright file="Backend.cs" company="GRAU DATA AG">
//
//   CmisSync, a collaboration and sharing tool.
//   Copyright (C) 2010  Hylke Bons &lt;hylkebons@gmail.com&gt;
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

namespace CmisSync.Lib
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Information about the version of CmisSync.Lib and the version of the operating system.
    /// </summary>
    public static class Backend
    {
        private static DateTime? linkerTimestamp = null;
        private static object singletonLock = new Object();

        /// <summary>
        /// Gets the Version of CmisSync.Lib
        /// It is also used as the CmisSync version.
        /// </summary>
        public static string Version {
            get {
                return string.Empty + Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        public static DateTime? RetrieveLinkerTimestamp {
            get {
                if (linkerTimestamp == null) {
                    lock(singletonLock) {
                        string filePath = Assembly.GetExecutingAssembly().Location;
                        const int c_PeHeaderOffset = 60;
                        const int c_LinkerTimestampOffset = 8;
                        byte[] b = new byte[2048];
                        try {
                            using (Stream s = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
                                s.Read(b, 0, 2048);
                            }
                        } catch (Exception) {
                            return null;
                        }

                        int i = System.BitConverter.ToInt32(b, c_PeHeaderOffset);
                        int secondsSince1970 = System.BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
                        DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        dt = dt.AddSeconds(secondsSince1970);
                        dt = dt.ToLocalTime();
                        linkerTimestamp = dt;
                    }
                }

                return linkerTimestamp;
            }
        }

        /// <summary>
        /// Gets the PlatformID.
        /// This fixes the PlatformID enumeration for MacOSX in Environment.OSVersion.Platform,
        /// which is intentionally broken in Mono for historical reasons
        /// </summary>
        public static PlatformID Platform {
            get {
                IntPtr buf = IntPtr.Zero;

                try {
                    buf = Marshal.AllocHGlobal(8192);

                    if (uname(buf) == 0 && Marshal.PtrToStringAnsi(buf) == "Darwin") {
                        return PlatformID.MacOSX;
                    }
                } catch (OutOfMemoryException) {
                } catch (DllNotFoundException) {
                } finally {
                    if (buf != IntPtr.Zero) {
                        Marshal.FreeHGlobal(buf);
                    }
                }

                return Environment.OSVersion.Platform;
            }
        }

        /// <summary>
        /// Import uname from libc for use in the previous method.
        /// </summary>
        /// <param name="buf">To write the uname result into</param>
        /// <returns>the error code</returns>
        [DllImport("libc")]
        private static extern int uname(IntPtr buf);
    }
}