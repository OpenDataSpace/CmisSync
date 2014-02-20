using System.IO;
namespace CmisSync.Lib.Storage 
{
    ///
    ///<summary>Wrapper for DirectoryInfo<summary>
    ///
    public abstract class FileSystemInfoWrapper : IFileSystemInfo
    {
        private FileSystemInfo fileSystemInfo;

        protected FileSystemInfoWrapper(FileSystemInfo fileSystemInfo)
        {
            this.fileSystemInfo = fileSystemInfo;
        }
        public string FullName{get {return fileSystemInfo.FullName;}}       
    }
}
