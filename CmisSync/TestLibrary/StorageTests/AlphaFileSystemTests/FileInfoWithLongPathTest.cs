//-----------------------------------------------------------------------
// <copyright file="FileInfoWithLongPathTest.cs" company="GRAU DATA AG">
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
﻿
namespace TestLibrary.StorageTests.AlphaFSTests {
    using System;

    using Alphaleonis.Win32.Filesystem;

    using NUnit.Framework;

    [TestFixture]
    public class FileInfoWithLongPathTest {
        [SetUp]
        public void IgnoreOnNonWindowsSystems() {
            #if __MonoCS__
            Assert.Ignore("Only on windows systems");
            #endif
        }

        #if !__MonoCS__
        [Test, Category("Medium")]
        public void CreateInstance() {
            var file = new FileInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()), PathFormat.FullPath);
            Assert.That(file.Exists, Is.False);
        }

        [Test, Category("Medium")]
        public void WriteADS() {
            var fileName = Guid.NewGuid().ToString();
            var adsName = "DSS-Test";
            File.WriteAllText(Path.Combine(Path.GetTempPath(), fileName + ":" + adsName), fileName, PathFormat.FullPath);

            var file = new FileInfo(Path.Combine(Path.GetTempPath(), fileName), PathFormat.FullPath);
            Assert.That(file.Exists, Is.True);
            int i = 0;
            foreach (var stream in file.EnumerateAlternateDataStreams()) {
                i++;
                Assert.That(stream.StreamName, Is.EqualTo(string.Empty).Or.EqualTo(adsName));
                if (stream.StreamName == adsName) {
                    Assert.That(stream.Size, Is.EqualTo(fileName.Length));
                } else {
                    Assert.That(stream.Size, Is.EqualTo(0));
                }
            }

            Assert.That(i, Is.EqualTo(2));
        }
        #endif
    }
}