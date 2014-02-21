using System;
using System.IO;

namespace CmisSync.Lib.Storage
{
    ///
    ///<summary>Wrapps all interfaced methods and calls the Systems.IO classes<summary>
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
