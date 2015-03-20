//-----------------------------------------------------------------------
// <copyright file="RepositoryRootDeletedDetectionTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.FilterTests {
    using System;
    using System.IO;

    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Filter;
    using CmisSync.Lib.Storage.FileSystem;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class RepositoryRootDeletedDetectionTest {
        [Test, Category("Fast")]
        public void ConstructorFailsIfGivenPathIsNull() {
            Assert.Throws<ArgumentNullException>(() => new RepositoryRootDeletedDetection(null));
        }

        [Test, Category("Fast")]
        public void ConstructorTakesPath() {
            var path = Mock.Of<IDirectoryInfo>(p => p.FullName == Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()) && p.Exists == true);
            var underTest = new RepositoryRootDeletedDetection(path);

            Assert.That(underTest.Priority, Is.EqualTo(EventHandlerPriorities.CRITICAL));
        }
    }
}