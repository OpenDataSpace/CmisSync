using System.IO;
namespace CmisSync.Lib.Storage 
{
    ///
    ///<summary>Wrapper for DirectoryInfo<summary>
    ///
    public class DirectoryInfoWrapper : FileSystemInfoWrapper, IDirectoryInfo
    {
        private DirectoryInfo original; 

        public DirectoryInfoWrapper(DirectoryInfo directoryInfo) 
            : base(directoryInfo)
        {
            this.original = directoryInfo;
        }

        public void Create() {
            original.Create();
        }

    }
}
