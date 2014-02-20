using System.IO;
namespace CmisSync.Lib.Storage 
{
    ///
    ///<summary>Wrapper for DirectoryInfo<summary>
    ///
    public abstract class FileSystemInfoWrapper : IFileSystemInfo
    {
        private FileSystemInfo original;

        protected FileSystemInfoWrapper(FileSystemInfo original)
        {
            this.original = original;
        }

        public string FullName {get {return original.FullName;}}       

        public bool Exists {get {return original.Exists;}}

        public void Refresh() {
            original.Refresh();
        }
    }
}
