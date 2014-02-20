using System.IO;
namespace CmisSync.Lib.Storage 
{
    ///
    ///<summary>Wrapper for DirectoryInfo<summary>
    ///
    public class DirectoryInfoWrapper : FileSystemInfoWrapper, IDirectoryInfo
    {
        public DirectoryInfoWrapper(DirectoryInfo directoryInfo) 
            : base(directoryInfo)
        {
        }
    }
}
