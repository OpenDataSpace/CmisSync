
namespace CmisSync.Lib.Filter {
    using System;

    using CmisSync.Lib.Storage.FileSystem;

    public class SymlinkFilter {
        public bool IsSymlink(IFileSystemInfo fsInfo, out string reason) {
            reason = string.Empty;
            if (fsInfo.Exists && fsInfo.IsSymlink) {
                reason = string.Format("{0} is a symbolic link", fsInfo.FullName);
                return true;
            } else {
                return false;
            }
        }
    }
}