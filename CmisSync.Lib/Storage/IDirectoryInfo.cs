namespace CmisSync.Lib.Storage 
{
    ///
    ///<summary>Interface to enable mocking of DirectoryInfo<summary>
    ///
    public interface IDirectoryInfo : IFileSystemInfo
    {
        void Create();
        IDirectoryInfo Parent { get; }
        IDirectoryInfo[] GetDirectories();
        IFileInfo[] GetFiles();
    }
}
