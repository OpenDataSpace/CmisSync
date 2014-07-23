//-----------------------------------------------------------------------
// <copyright file="AbstractEnhancedSolverTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.ConsumerTests.SituationSolverTests
{
    using System;

    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class AbstractEnhancedSolverTest
    {
        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorThrowsExceptionIfSessionIsNull() {
            Assert.Throws<ArgumentNullException>(() => new SolverClass(null, Mock.Of<IMetaDataStorage>()));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorThrowsExceptionIfStorageIsNull() {
            Assert.Throws<ArgumentNullException>(() => new SolverClass(Mock.Of<ISession>(), null));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorSetsPropertiesCorrectly() {
            var session = Mock.Of<ISession>();
            var storage = Mock.Of<IMetaDataStorage>();
            var solver = new SolverClass(session, storage);
            Assert.That(solver.GetSession(), Is.EqualTo(session));
            Assert.That(solver.GetStorage(), Is.EqualTo(storage));
        }

        private class SolverClass : AbstractEnhancedSolver {
            public SolverClass(ISession session, IMetaDataStorage storage) : base(session, storage) {
            }

            public ISession GetSession() {
                return this.Session;
            }

            public IMetaDataStorage GetStorage() {
                return this.Storage;
            }

            public override void Solve(IFileSystemInfo localFileSystemInfo, IObjectId remoteId) {
                throw new NotImplementedException();
            }
        }
    }
}