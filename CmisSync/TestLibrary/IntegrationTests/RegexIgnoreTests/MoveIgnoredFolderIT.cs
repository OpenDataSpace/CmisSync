//-----------------------------------------------------------------------
// <copyright file="MoveIgnoredFolderIT.cs" company="GRAU DATA AG">
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
namespace TestLibrary.IntegrationTests.RegexIgnoreTests {
    using System;
    using System.IO;
    using System.Linq;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS.Client;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture,  TestName("MoveIgnoredFolder"), Category("RegexIgnore"), Category("Slow"), Timeout(180000)]
    public class MoveIgnoredFolderIT : BaseRegexIgnoreTest {
        [Test, Ignore("TODO")]
        public void MoveIgnoredLocalFolderToAnotherIgnoredLocalFolder() {
        }

        [Test, Ignore("TODO")]
        public void MoveIgnoredLocalFolderToAnotherFolder() {
        }

        [Test, Ignore("TODO")]
        public void MoveLocalFolderToIgnoredLocalFolder() {
        }
    }
}