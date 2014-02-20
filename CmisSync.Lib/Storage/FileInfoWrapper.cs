using System.IO;
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
            this.original = fileInfo;
        }

        public IDirectoryInfo Directory {
            get { 
                return new DirectoryInfoWrapper(original.Directory);
            } 
        }
    }
}
