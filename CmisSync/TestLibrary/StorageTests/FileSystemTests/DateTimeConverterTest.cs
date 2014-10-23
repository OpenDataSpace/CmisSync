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
                if (objDrive.IsReady)
                {
                    Console.WriteLine("Drive Name :   " + objDrive.Name);
                    Console.WriteLine("Drive Format : " + objDrive.DriveFormat);
                    Console.WriteLine("");
                }
            }
        }

        [Test, Category("Fast")]
        public void CreateOldDateTime() {
            new DateTime(1601, 1, 1);
        }

        [Test, Category("Fast")]
        public void CreateFutureTime() {
            new DateTime(5000, 1, 1);
        }
    }
}