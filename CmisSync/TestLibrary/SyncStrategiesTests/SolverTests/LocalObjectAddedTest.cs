//-----------------------------------------------------------------------
// <copyright file="LocalObjectAddedTest.cs" company="GRAU DATA AG">
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
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using CmisSync.Lib.Sync.Solver;
using CmisSync.Lib.Storage;

using DotCMIS.Client;

using Moq;

using NUnit.Framework;
using DotCMIS.Data;
using DotCMIS.Enums;

namespace TestLibrary.SyncStrategiesTests.SolverTests
{
    [TestFixture]
    public class LocalObjectAddedTest
    {
        private Mock<ISession> Session;
        private Mock<IMetaDataStorage> Storage;

        [SetUp]
        public void SetUp()
        {
            Session = new Mock<ISession>();
            Storage = new Mock<IMetaDataStorage>();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new LocalObjectAdded();
        }

    }
}

