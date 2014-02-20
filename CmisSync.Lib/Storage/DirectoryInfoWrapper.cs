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

        public IDirectoryInfo Parent {
            get { 
                return new DirectoryInfoWrapper(original.Parent);
            } 
        }

        public IDirectoryInfo[] GetDirectories() {
            DirectoryInfo[] originalDirs = original.GetDirectories();
            IDirectoryInfo[] wrappedDirs = new IDirectoryInfo[originalDirs.Length];
            for(int i = 0; i < originalDirs.Length; i++){
                wrappedDirs[i] = new DirectoryInfoWrapper(originalDirs[i]);
            }
            return wrappedDirs;
        }

        public IFileInfo[] GetFiles() {
            FileInfo[] originalFiles = original.GetFiles();
            IFileInfo[] wrappedFiles = new IFileInfo[originalFiles.Length];
            for(int i = 0; i < originalFiles.Length; i++){
                wrappedFiles[i] = new FileInfoWrapper(originalFiles[i]);
            }
            return wrappedFiles;
        }
    }
}
