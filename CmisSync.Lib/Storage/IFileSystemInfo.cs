using System;
using System.IO;

namespace CmisSync.Lib.Storage
{
    ///
    ///<summary>Interface to enable mocking of FileSystemInfo<summary>
    ///
    public interface IFileSystemInfo
    {
        String FullName { get; }
        String Name { get; }
        bool Exists { get; }
        FileAttributes Attributes { get; }
        void Refresh ();
        void SetExtendedAttribute(string key, string value);
        string GetExtendedAttribute(string key);
    }
}
