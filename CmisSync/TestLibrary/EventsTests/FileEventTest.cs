//-----------------------------------------------------------------------
// <copyright file="FileEventTest.cs" company="GRAU DATA AG">
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
using NUnit.Framework;
using Moq;
using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;

namespace TestLibrary.EventsTests {
    [TestFixture]
    public class FileEventTest {
        [Test, Category("Fast")]
        public void ConstructorTakesIFileInfoInstance()
        {
            new FileEvent(new Mock<IFileInfo>().Object);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnNullParameter()
        {
            new FileEvent();
        }

        [Test, Category("Fast")]
        public void EqualityNull() {
            var localFile = new Mock<IFileInfo>();
            localFile.Setup(f => f.FullName).Returns("bla");
            var fe = new FileEvent(localFile.Object);
            Assert.That(fe, Is.Not.EqualTo(null));
        }

        [Test, Category("Fast")]
        public void EqualitySame() {
            var localFile = new Mock<IFileInfo>();
            localFile.Setup(f => f.FullName).Returns("bla");
            var fe = new FileEvent(localFile.Object);
            Assert.That(fe, Is.EqualTo(fe));
        }
    }
}
