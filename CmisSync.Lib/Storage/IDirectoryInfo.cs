namespace CmisSync.Lib.Storage
{
    ///
    ///<summary>Interface to enable mocking of DirectoryInfo<summary>
    ///
    public interface IDirectoryInfo : IFileSystemInfo
    {
        IDirectoryInfo Parent { get; }

        void Create ();
        IDirectoryInfo[] GetDirectories ();
        IFileInfo[] GetFiles ();
        void Delete (bool recursive);
    }
}
