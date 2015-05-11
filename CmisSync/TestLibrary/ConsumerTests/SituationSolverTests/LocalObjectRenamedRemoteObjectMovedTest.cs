//-----------------------------------------------------------------------
// <copyright file="LocalObjectRenamedRemoteObjectMovedTest.cs" company="GRAU DATA AG">
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
namespace TestLibrary.ConsumerTests.SituationSolverTests
{
    using System;

    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class LocalObjectRenamedRemoteObjectMovedTest
    {
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private TransmissionManager transmissionManager;
        private Mock<LocalObjectRenamedRemoteObjectRenamed> renameSolver;
        private Mock<LocalObjectChangedRemoteObjectChanged> changeSolver;

        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem();
            this.storage = new Mock<IMetaDataStorage>();
            this.transmissionManager = new TransmissionManager();
            this.changeSolver = new Mock<LocalObjectChangedRemoteObjectChanged>(this.session.Object, this.storage.Object, null, this.transmissionManager, Mock.Of<IFileSystemInfoFactory>());
            this.renameSolver = new Mock<LocalObjectRenamedRemoteObjectRenamed>(this.session.Object, this.storage.Object, this.changeSolver.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructor() {
            new LocalObjectRenamedRemoteObjectMoved(this.session.Object, Mock.Of<IMetaDataStorage>(), this.renameSolver.Object, this.changeSolver.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorFailsIfNoRenameSolverIsPassed() {
            Assert.Throws<ArgumentNullException>(() => new LocalObjectRenamedRemoteObjectMoved(this.session.Object, Mock.Of<IMetaDataStorage>(), null, this.changeSolver.Object));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorFailsIfNoChangeSolverIsPassed() {
            Assert.Throws<ArgumentNullException>(() => new LocalObjectRenamedRemoteObjectMoved(this.session.Object, Mock.Of<IMetaDataStorage>(), this.renameSolver.Object, null));
        }
    }
}