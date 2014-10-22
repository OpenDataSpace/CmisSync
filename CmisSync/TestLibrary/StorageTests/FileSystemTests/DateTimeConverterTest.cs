using System.IO;

namespace TestLibrary.StorageTests.FileSystemTests
{
    using System;

    using NUnit.Framework;

    using Moq;

    [TestFixture]
    public class DateTimeConverterTest
    {
        [Test, Category("Medium"), Ignore("https://bugzilla.xamarin.com/show_bug.cgi?id=23933")]
        public void RequestingDriveType() {
            foreach (DriveInfo objDrive in DriveInfo.GetDrives())
            {
                Console.WriteLine("Drive Name :   " + objDrive.Name);
                Console.WriteLine("Drive Format : " + objDrive.DriveFormat);
                Console.WriteLine("");
            }
        }
    }
}