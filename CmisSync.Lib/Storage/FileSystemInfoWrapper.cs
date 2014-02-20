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

        public string Name {get {return original.Name;}}       
        public bool Exists {get {return original.Exists;}}

        public FileAttributes Attributes {get {return original.Attributes;}}

        public void Refresh() {
            original.Refresh();
        }
    }
}
