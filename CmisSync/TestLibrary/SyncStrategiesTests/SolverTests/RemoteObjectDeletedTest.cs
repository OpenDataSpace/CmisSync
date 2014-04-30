//-----------------------------------------------------------------------
// <copyright file="RemoteObjectDeletedTest.cs" company="GRAU DATA AG">
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
    using System.IO;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync.Solver;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class RemoteObjectDeletedTest
    {
        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new RemoteObjectDeleted();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFolderDeleted()
        {
            string path = Path.Combine(Path.GetTempPath(), "a");
            var session = new Mock<ISession>();
            var storage = new Mock<IMetaDataStorage>();
            Mock<IMappedObject> folder = storage.AddLocalFolder(path, "id");
            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.FullName).Returns(path);
            var solver = new RemoteObjectDeleted();
            solver.Solve(session.Object, storage.Object, dirInfo.Object, null);

            dirInfo.Verify(d => d.Delete(true), Times.Once());
            storage.Verify(s => s.RemoveObject(It.Is<IMappedObject>(o => o == folder.Object)), Times.Once());
        }
    }
}