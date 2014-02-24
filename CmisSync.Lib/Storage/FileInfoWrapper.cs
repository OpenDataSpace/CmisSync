using System.IO;
using System;


namespace CmisSync.Lib.Storage
{
    ///
    ///<summary>Wrapper for FileInfo<summary>
    ///
    public class FileInfoWrapper : FileSystemInfoWrapper, IFileInfo
    {
        private FileInfo original;

        public FileInfoWrapper(FileInfo fileInfo)
            : base(fileInfo)
        {
            original = fileInfo;
        }

        public IDirectoryInfo Directory {
            get {
                return new DirectoryInfoWrapper(original.Directory);
            }
        }

        public long Length {
            get {
                return original.Length;
            }
        }

        public DateTime LastWriteTimeUtc {
            get {
                return original.LastWriteTimeUtc;
            }
        }

        public DateTime LastWriteTime {
            get {
                return original.LastWriteTime;
            }
        }

        public Stream Open(FileMode mode)
        {
            return original.Open(mode);
        }

        public Stream Open(FileMode mode, FileAccess access)
        {
            return original.Open(mode, access);
        }

        public Stream Open(FileMode mode, FileAccess access, FileShare share)
        {
            return original.Open(mode, access, share);
        }
    }
}
