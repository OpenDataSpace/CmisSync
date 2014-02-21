using System;
using System.IO;

namespace CmisSync.Lib.Storage
{
    ///
    ///<summary>Interface for Creating IFileSystemInfo Implementations <summary>
    ///
    public class FileSystemInfoFactory : IFileSystemInfoFactory
    {
        public IDirectoryInfo CreateDirectoryInfo (string path)
        {
            return new DirectoryInfoWrapper (new DirectoryInfo (path));
        }

        public IFileInfo CreateFileInfo (string path)
        {
            return new FileInfoWrapper (new FileInfo (path));
        }
    }
}
