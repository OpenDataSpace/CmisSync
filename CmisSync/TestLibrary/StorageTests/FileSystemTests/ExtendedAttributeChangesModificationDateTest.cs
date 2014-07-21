//-----------------------------------------------------------------------
// <copyright file="ExtendedAttributeChangesModificationDateTest.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Storage.FileSystem;

    using NUnit.Framework;

    using TestLibrary.IntegrationTests;

    [TestFixture]
    public class ExtendedAttributeChangesModificationDateTest
    {
        private string path = string.Empty;
        private IFileSystemInfoFactory fsFactory = new FileSystemInfoFactory();

        [SetUp]
        public void SetUp() {
            var config = ITUtils.GetConfig();
            string localPath = config[1].ToString();
            this.path = Path.Combine(localPath, Path.GetRandomFileName());
            if (!this.fsFactory.CreateDirectoryInfo(localPath).IsExtendedAttributeAvailable()) {
                Assert.Ignore("Extended Attribute not available on this machine");
            }
        }

        [TearDown]
        public void TearDown() {
            if (File.Exists(this.path)) {
                File.Delete(this.path);
            }

            if (Directory.Exists(this.path)) {
                Directory.Delete(this.path);
            }
        }

        [Test, Category("Medium")]
        public void SetExtendedAttributeToFileDoesNotChangesModificationDate()
        {
            var file = this.fsFactory.CreateFileInfo(this.path);
            using (file.Open(FileMode.CreateNew)) {
            }
            DateTime oldTime = DateTime.UtcNow.AddDays(1);
            file.LastWriteTimeUtc = oldTime;
            file.Refresh();
            oldTime = file.LastWriteTimeUtc;

            file.SetExtendedAttribute("Test", "test");

            file.Refresh();
            Assert.That(file.LastWriteTimeUtc, Is.EqualTo(oldTime));
        }
    }
}