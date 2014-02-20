using System;

namespace CmisSync.Lib.Storage 
{
    ///
    ///<summary>Interface for Creating IFileSystemInfo Implementations <summary>
    ///
    public class FileSystemInfoFactory : IFileSystemInfoFactory
    {
        public IDirectoryInfo CreateDirectoryInfo(string path) {
            return new DirectoryInfoWrapper();
        }

        public IFileInfo CreateFileInfo(string path) {
            return new FileInfoWrapper();
        }
    }
}
