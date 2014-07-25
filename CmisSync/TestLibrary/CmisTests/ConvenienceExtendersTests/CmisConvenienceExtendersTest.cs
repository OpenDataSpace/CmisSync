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

namespace TestLibrary.CmisTests.ConvenienceExtendersTests
{
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class CmisConvenienceExtendersTest
    {
        [Test, Category("Fast")]
        public void ContentStreamHashReturnNullIfNoHashIsAvailable() {
            var doc = Mock.Of<IDocument>(
                d =>
                d.Properties == new List<IProperty>());
            Assert.That(doc.ContentStreamHash(), Is.Null);
        }

        [Test, Category("Fast")]
        public void ContentStreamHashReturnsNullIfNoPropertiesAreAvailable() {
            var doc = Mock.Of<IDocument>();
            Assert.That(doc.ContentStreamHash(), Is.Null);
        }

        [Test, Category("Fast")]
        public void ContentStreamHashReturnsByteArrayIfHashIsAvailable() {
            var doc = new Mock<IDocument>();
            byte[] hash = new byte[20];
            doc.SetupContentStreamHash(hash);
            Assert.That(doc.Object.ContentStreamHash(), Is.EqualTo(hash));
        }

        [Test, Category("Fast")]
        public void ContentStreamHashReturnsByteArrayIfHashIsAvailableAndTypeIsGiven() {
            var properties = new List<IProperty>();
            IList<object> values = new List<object>();
            values.Add("{md5}00");
            var property = Mock.Of<IProperty>(
                p =>
                p.IsMultiValued == true &&
                p.Id == "cmis:contentStreamHash" &&
                p.Values == values);

            properties.Add(property);
            var doc = Mock.Of<IDocument>(
                d =>
                d.Properties == properties);
            Assert.That(doc.ContentStreamHash("MD5"), Is.EqualTo(new byte[1]));
        }
    }
}