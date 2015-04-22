//-----------------------------------------------------------------------
// <copyright file="AbstractEnhancedSolverWithPWCTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.ConsumerTests.SituationSolverTests.PrivateWorkingCopyTests {
    using System;

    using CmisSync.Lib.Consumer.SituationSolver.PWC;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class AbstractEnhancedSolverWithPWCTest {
        [Test, Category("Fast")]
        public void ConstructorThrowsExceptionIfTransmissionStorageIsNull() {
            var session = new Mock<ISession>().SetupTypeSystem().SetupPrivateWorkingCopyCapability().Object;
            Assert.Throws<ArgumentNullException>(() => new SolverClass(session, Mock.Of<IMetaDataStorage>(), null));
        }

        [Test, Category("Fast")]
        public void ConstructorThrowsExceptionIfSessionDoesNotSupportPwc() {
            var session = new Mock<ISession>().SetupTypeSystem().SetupPrivateWorkingCopyCapability(false).Object;
            Assert.Throws<ArgumentException>(() => new SolverClass(session, Mock.Of<IMetaDataStorage>(), Mock.Of<IFileTransmissionStorage>()));
        }

        private class SolverClass : AbstractEnhancedSolverWithPWC {
            public SolverClass(
                ISession session,
                IMetaDataStorage storage,
                IFileTransmissionStorage transmissionStorage) : base(session, storage, transmissionStorage) {
            }

            public IFileTransmissionStorage GetTransmissionStorage() {
                return this.TransmissionStorage;
            }

            public override void Solve(
                IFileSystemInfo localFileSystemInfo,
                IObjectId remoteId,
                ContentChangeType localContent,
                ContentChangeType remoteContent)
            {
                throw new NotImplementedException();
            }
        }
    }
}