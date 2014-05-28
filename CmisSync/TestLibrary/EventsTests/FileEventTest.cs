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

namespace TestLibrary.EventsTests
{
    using System;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class FileEventTest
    {
        [Test, Category("Fast")]
        public void ConstructorTakesIFileInfoInstance()
        {
            var file = Mock.Of<IFileInfo>();
            var ev = new FileEvent(file);
            Assert.That(ev.LocalFile, Is.EqualTo(file));
        }

        [Test, Category("Fast")]
        public void ConstructorTakesRemoteFile()
        {
            var file = Mock.Of<IDocument>();
            var ev = new FileEvent(null, file);
            Assert.That(ev.RemoteFile, Is.EqualTo(file));
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnNullParameter()
        {
            new FileEvent();
        }

        [Test, Category("Fast")]
        public void EqualityNull()
        {
            var localFile = Mock.Of<IFileInfo>();
            var fe = new FileEvent(localFile);
            Assert.That(fe.RemoteFile, Is.Null);
            Assert.That(fe, Is.Not.EqualTo(null));
        }

        [Test, Category("Fast")]
        public void EqualitySame()
        {
            var remoteFile = Mock.Of<IDocument>();
            var fe = new FileEvent(null, remoteFile);
            Assert.That(fe.LocalFile, Is.Null);
            Assert.That(fe, Is.EqualTo(fe));
        }
    }
}
