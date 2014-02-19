using System;
using System.IO;
using System.Text;

using Mono.Unix.Native;

using MonoMac.Foundation;

using log4net;

namespace CmisSync.Notifications
{
    public static class FileSystemProgress
    {
        private static readonly string extendAttrKey = "com.apple.progress.fractionCompleted";
        private static readonly ILog Logger = LogManager.GetLogger(typeof(FileSystemProgress));
        /// <summary>
        /// Removes the file progress if available.
        /// </summary>
        /// <param name="path">Path of the file/folder</param>
        public static void RemoveFileProgress(string path)
        {
            try {
                Syscall.removexattr (path, extendAttrKey);
                NSFileAttributes attr = NSFileManager.DefaultManager.GetAttributes (path);
                attr.CreationDate = (new FileInfo(path)).CreationTime;
                NSFileManager.DefaultManager.SetAttributes (attr, path);
            } catch (Exception ex) {
                Logger.Debug (String.Format ("Exception to unset {0} creation time for file status update: {1}", path, ex));
            }
        }

        public static void SetFileProgress(string path, double percent)
        {
            try {
                Syscall.setxattr (path, extendAttrKey, Encoding.ASCII.GetBytes (percent.ToString ()));
                NSFileAttributes attr = NSFileManager.DefaultManager.GetAttributes (path);
                attr.CreationDate = new DateTime (1984, 1, 24, 8, 0, 0, DateTimeKind.Utc);
                NSFileManager.DefaultManager.SetAttributes (attr, path);
            } catch (Exception ex) {
                Logger.Debug (String.Format ("Exception to set {0} creation time for file status update: {1}", path, ex));
            }
        }
    }
}

