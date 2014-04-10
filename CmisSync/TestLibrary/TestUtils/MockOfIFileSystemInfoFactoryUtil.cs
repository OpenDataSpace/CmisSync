using System;
using System.IO;

using CmisSync.Lib.Storage;

using Moq;

namespace TestLibrary.TestUtils
{
    public static class MockOfIFileSystemInfoFactoryUtil
    {
        public static void AddIDirectoryInfo(this Mock<IFileSystemInfoFactory> fsFactory, IDirectoryInfo dirInfo)
        {
            fsFactory.Setup(f => f.CreateDirectoryInfo(dirInfo.FullName)).Returns(dirInfo);
        }

        public static void AddDirectory(this Mock<IFileSystemInfoFactory> fsFactory, string path, bool exists = true )
        {
            fsFactory.AddIDirectoryInfo(Mock.Of<IDirectoryInfo>(d =>
                                                                d.FullName == path &&
                                                                d.Exists == exists));
        }

        public static void AddDirectoryWithParents(this Mock<IFileSystemInfoFactory> fsFactory, string path)
        {
            if(path.Length > 0)
            fsFactory.AddDirectory(path);
            int lastSeperator = path.LastIndexOf(Path.DirectorySeparatorChar.ToString());
            if(lastSeperator > 0)
            {
                fsFactory.AddDirectoryWithParents(path.Substring(lastSeperator));
            }
        }

        public static void AddIFileInfo(this Mock<IFileSystemInfoFactory> fsFactory, IFileInfo fileInfo)
        {
            fsFactory.Setup(f => f.CreateFileInfo(fileInfo.FullName)).Returns(fileInfo);
        }

        public static void AddFile(this Mock<IFileSystemInfoFactory> fsFactory, string path, bool exists = true )
        {
            fsFactory.AddIFileInfo(Mock.Of<IFileInfo>(d =>
                                                                d.FullName == path &&
                                                                d.Exists == exists));
        }
    }
}

