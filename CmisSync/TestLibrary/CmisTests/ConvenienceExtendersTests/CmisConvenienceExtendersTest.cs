//-----------------------------------------------------------------------
// <copyright file="CmisConvenienceExtendersTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.CmisTests.ConvenienceExtendersTests {
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture, Category("Fast")]
    public class CmisConvenienceExtendersTest {
        [Test]
        public void ContentStreamHashReturnNullIfNoHashIsAvailable() {
            var doc = Mock.Of<IDocument>(
                d =>
                d.Properties == new List<IProperty>());
            Assert.That(doc.ContentStreamHash(), Is.Null);
        }

        [Test]
        public void ContentStreamHashReturnsNullIfNoPropertiesAreAvailable() {
            var doc = Mock.Of<IDocument>();
            Assert.That(doc.ContentStreamHash(), Is.Null);
        }

        [Test]
        public void ContentStreamHashReturnsByteArrayIfHashIsAvailable() {
            var doc = new Mock<IDocument>();
            byte[] hash = new byte[20];
            doc.SetupContentStreamHash(hash);
            Assert.That(doc.Object.ContentStreamHash(), Is.EqualTo(hash));
        }

        [Test]
        public void ContentStreamHashReturnsByteArrayIfHashIsAvailableAndTypeIsGiven() {
            var doc = new Mock<IDocument>();
            byte[] hash = new byte[16];
            doc.SetupContentStreamHash(hash, "MD5");
            Assert.That(doc.Object.ContentStreamHash("MD5"), Is.EqualTo(hash));
        }

        [Test]
        public void ContentStreamHashReturnsNullIfNoValueInPropertyIsAvailable() {
            var properties = new List<IProperty>();
            var property = Mock.Of<IProperty>(
                p =>
                p.IsMultiValued == true &&
                p.Id == "cmis:contentStreamHash");

            properties.Add(property);
            var doc = Mock.Of<IDocument>(
                d =>
                d.Properties == properties);
            Assert.That(doc.ContentStreamHash(), Is.Null);
        }

        [Test]
        public void PrivateWorkingCopyIsUpdateable([Values(true, false)]bool updateable) {
            var session = new Mock<ISession>();
            session.SetupPrivateWorkingCopyCapability(updateable);
            Assert.That(session.Object.IsPrivateWorkingCopySupported(), Is.EqualTo(updateable));
        }

        [Test]
        public void PrivateWorkingCopyIsUpdateableReturnsFalseOnException() {
            var session = new Mock<ISession>();
            Assert.That(session.Object.IsPrivateWorkingCopySupported(), Is.False);
        }
    }
}