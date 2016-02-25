//-----------------------------------------------------------------------
// <copyright file="WindowsNamesIT.cs" company="GRAU DATA AG">
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
namespace TestLibrary.IntegrationTests.ValidNamesTests {
    using System;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture, TestName("WindowsFileNames"), Category("Names"), Category("Slow"), Timeout(180000)]
    public class WindowsNamesIT : BaseFullRepoTest {
        [Test, Ignore("Fails")]
        public void CreateFile([Values("foo", ".foo", ".foo.", "foo.")]string validFileName) {
            var doc = this.remoteRootDir.CreateDocument(validFileName, string.Empty);
            Assert.That(doc.Name, Is.EqualTo(validFileName));
        }

        [Test, Ignore("Fails")]
        public void CreateDirectory([Values("foo", ".foo", ".foo.", "foo.")]string validDirName) {
            var dir = this.remoteRootDir.CreateFolder(validDirName);
            Assert.That(dir.Name, Is.EqualTo(validDirName));
        }
    }
}