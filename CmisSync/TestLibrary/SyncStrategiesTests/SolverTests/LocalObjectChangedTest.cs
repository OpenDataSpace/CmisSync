//-----------------------------------------------------------------------
// <copyright file="LocalObjectChangedTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.SyncStrategiesTests.SolverTests
{
    using System;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync.Solver;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class LocalObjectChangedTest
    {
        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new LocalObjectChanged();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFolderChanged()
        {
            var modificationDate = DateTime.UtcNow;
            var solver = new LocalObjectChanged();
            var storage = new Mock<IMetaDataStorage>();
            var localDirectory = Mock.Of<IDirectoryInfo>(
                f =>
                f.LastWriteTimeUtc == modificationDate);
            var remoteDirectory = new Mock<IFolder>();

            var mappedObject = new MappedObject(
                "name",
                "remoteId",
                MappedObjectType.Folder,
                "parentId",
                "changeToken")
            {
                Guid = Guid.NewGuid(),
                LastRemoteWriteTimeUtc = modificationDate
            };
            storage.AddMappedFolder(mappedObject);

            solver.Solve(Mock.Of<ISession>(), storage.Object, localDirectory, remoteDirectory.Object);

            storage.VerifySavedMappedObject(
                MappedObjectType.Folder,
                "remoteId",
                mappedObject.Name,
                mappedObject.ParentId,
                mappedObject.LastChangeToken,
                true,
                localDirectory.LastWriteTimeUtc);
        }

        [Ignore]
        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileModificationDateChanged()
        {
            Assert.Fail("TODO");
        }

        [Ignore]
        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileContentChanged()
        {
            Assert.Fail("TODO");
        }
    }
}