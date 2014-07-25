
namespace TestLibrary.CmisTests.ConvenienceExtendersTests
{
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

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
            var properties = new List<IProperty>();
            IList<object> values = new List<object>();
            values.Add("{sha-1}00");
            var property = Mock.Of<IProperty>(
                p =>
                p.IsMultiValued == true &&
                p.Id == "cmis:contentStreamHash" &&
                p.Values == values);

            properties.Add(property);
            var doc = Mock.Of<IDocument>(
                d =>
                d.Properties == properties);
            Assert.That(doc.ContentStreamHash(), Is.EqualTo(new byte[1]));
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