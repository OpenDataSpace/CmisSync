//-----------------------------------------------------------------------
// <copyright file="DateTimeConverterTest.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------

namespace TestLibrary.StorageTests.FileSystemTests
{
    using System;
    using System.IO;

    using Moq;

    using NUnit.Framework;

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
                    Console.WriteLine(string.Empty);
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