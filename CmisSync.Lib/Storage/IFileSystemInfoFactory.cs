using System;

namespace CmisSync.Lib.Storage 
{
    ///
    ///<summary>Interface for Creating IFileSystemInfo Implementations <summary>
    ///
    public interface IFileSystemInfoFactory
    {
        IDirectoryInfo CreateDirectoryInfo(string path);
        IFileInfo CreateFileInfo(string fileName);
    }
}
