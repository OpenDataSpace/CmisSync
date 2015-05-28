//-----------------------------------------------------------------------
// <copyright file="FullInMemoryRepoTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.IntegrationTests {
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib;
    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Queueing;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Client.Impl;

    using Moq;
    using NUnit.Framework;

    using TestLibrary.MockedServer;
    using TestLibrary.TestUtils;

    [TestFixture, Timeout(900000), Ignore, TestName("FullInMemoryRepoTests")]
    public class FullInMemoryRepoTest : BaseFullRepoTest {
        [Test, Category("Medium")]
        public void TestToConnect() {
        }
    }
}