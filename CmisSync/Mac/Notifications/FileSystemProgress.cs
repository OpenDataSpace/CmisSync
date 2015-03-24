//-----------------------------------------------------------------------
// <copyright file="FileSystemProgress.cs" company="GRAU DATA AG">
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

namespace CmisSync.Notifications {
    using System;
    using System.IO;
    using System.Text;

    using Mono.Unix.Native;

    using MonoMac.Foundation;

    using log4net;

    public static class FileSystemProgress {
        private static readonly string extendAttrKey = "com.apple.progress.fractionCompleted";
        private static readonly ILog Logger = LogManager.GetLogger(typeof(FileSystemProgress));
        /// <summary>
        /// Removes the file progress if available.
        /// </summary>
        /// <param name="path">Path of the file/folder</param>
        public static void RemoveFileProgress(string path) {
            try {
                Syscall.removexattr(path, extendAttrKey);
                NSFileAttributes attr = NSFileManager.DefaultManager.GetAttributes(path);
                attr.CreationDate = (new FileInfo(path)).CreationTime;
                NSFileManager.DefaultManager.SetAttributes(attr, path);
            } catch (Exception ex) {
                Logger.Debug(String.Format("Exception to unset {0} creation time for file status update: {1}", path, ex));
            }
        }

        public static void SetFileProgress(string path, double percent) {
            try {
                Syscall.setxattr(path, extendAttrKey, Encoding.ASCII.GetBytes(percent.ToString()));
                NSFileAttributes attr = NSFileManager.DefaultManager.GetAttributes(path);
                attr.CreationDate = new DateTime(1984, 1, 24, 8, 0, 0, DateTimeKind.Utc);
                NSFileManager.DefaultManager.SetAttributes(attr, path);
            } catch (Exception ex) {
                Logger.Debug(string.Format("Exception to set {0} creation time for file status update: {1}", path, ex));
            }
        }
    }
}