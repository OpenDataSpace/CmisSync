using System;
using System.IO;

namespace CmisSync.Lib.Storage
{
    ///
    ///<summary>Interface to enable mocking of FileInfo<summary>
    ///
    public interface IFileInfo : IFileSystemInfo
    {
        IDirectoryInfo Directory { get; }
        DateTime LastWriteTimeUtc { get; }
        DateTime LastWriteTime { get; }
        long Length { get; }
        Stream Open (FileMode open);
        Stream Open (FileMode open, FileAccess access);
        Stream Open (FileMode open, FileAccess access, FileShare share);
    }
}
