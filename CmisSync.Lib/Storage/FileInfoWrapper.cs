using System.IO;
namespace CmisSync.Lib.Storage 
{
    ///
    ///<summary>Wrapper for FileInfo<summary>
    ///
    public class FileInfoWrapper : FileSystemInfoWrapper, IFileInfo
    {
        public FileInfoWrapper(FileInfo fileInfo) 
            : base(fileInfo)
        {
        }
    }
}
