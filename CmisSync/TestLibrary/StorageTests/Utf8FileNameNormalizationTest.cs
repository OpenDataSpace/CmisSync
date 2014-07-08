//-----------------------------------------------------------------------
// <copyright file="Utf8FileNameNormalizationTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.IntegrationTests
{
    using System;
    using System.IO;

    using NUnit.Framework;

    [TestFixture]
    public class Utf8FileNameNormalizationTest
    {
        private string directoryPath;

        [SetUp]
        public void createDirectory() {
            this.directoryPath = Path.Combine(Path.GetTempPath(), "umlaut-test");
            Directory.CreateDirectory(this.directoryPath);
        }

        [TearDown]
        public void removeDirectory() {
            Directory.Delete(directoryPath, true);
        }

        [Ignore]
        [Test, Category("Medium"), Category("IT")]
        public void CreateFileWithUmlaut()
        {
            var filename = "ä";
            FileInfo info = new FileInfo(Path.Combine(this.directoryPath, filename));
            using (info.Create()) {
            };
            info.Refresh();
            Assert.That(info.Name, Is.EqualTo(filename));
            DirectoryInfo dirInfo = new DirectoryInfo(this.directoryPath);
            Assert.That(dirInfo.GetFiles()[0].Name, Is.EqualTo(filename));
        }
    }
}

