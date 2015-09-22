//-----------------------------------------------------------------------
// <copyright file="MockOfIDocumentUtil.cs" company="GRAU DATA AG">
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

namespace TestLibrary.TestUtils {
    using System;
    using System.Collections.Generic;
    using System.IO;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Data;

    using Moq;

    using NUnit.Framework;

    public static class MockOfIDocumentUtil {
        public static Mock<IDocument> CreateRemoteDocumentMock(string documentContentStreamId, string id, string name, string parentId, long contentLength = 0, byte[] content = null, string changeToken = "changetoken") {
            var newParentMock = Mock.Of<IFolder>(p => p.Id == parentId);
            return CreateRemoteDocumentMock(documentContentStreamId, id, name, newParentMock, contentLength, content, changeToken);
        }

        public static Mock<IDocument> CreateRemoteDocumentMock(string documentContentStreamId, string id, string name, IFolder parent, long contentLength = 0, byte[] content = null, string changeToken = "changetoken") {
            var newRemoteObject = new Mock<IDocument>();
            newRemoteObject.Setup(d => d.ContentStreamId).Returns(documentContentStreamId);
            newRemoteObject.Setup(d => d.ContentStreamLength).Returns(contentLength);
            newRemoteObject.Setup(d => d.Id).Returns(id);
            newRemoteObject.Setup(d => d.Name).Returns(name);
            newRemoteObject.Setup(d => d.Parents).Returns(new List<IFolder>() { parent });
            newRemoteObject.Setup(d => d.ChangeToken).Returns(changeToken);
            newRemoteObject.SetupContent(content, name);
            newRemoteObject.SetupParent(parent);
            return newRemoteObject;
        }

        public static void SetupContent(this Mock<IDocument> doc, byte[] content, string fileName, string mimeType = "application/octet-stream") {
            if (content != null) {
                var stream = Mock.Of<IContentStream>(
                    s =>
                    s.Length == content.Length &&
                    s.MimeType == mimeType &&
                    s.FileName == fileName &&
                    s.Stream == new MemoryStream(content));
                doc.Setup(d => d.GetContentStream()).Returns(stream);
                doc.Setup(d => d.GetContentStream(It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<long?>())).Callback((string id, long? offset, long? length) => {
                    stream.Stream.Seek((long)offset, SeekOrigin.Begin);
                    stream.Stream.SetLength((long)offset + (long)length);
                }).Returns(stream);
            }
        }

        public static Mock<IDocument> SetupParent(this Mock<IDocument> doc, IFolder parent) {
            List<IFolder> parents = new List<IFolder>();
            parents.Add(parent);
            doc.Setup(d => d.Parents).Returns(parents);
            return doc;
        }

        public static Mock<IDocument> SetupPath(this Mock<IDocument> doc, params string[] path) {
            List<string> paths = new List<string>(path);
            doc.Setup(d => d.Paths).Returns(paths);
            return doc;
        }

        public static Mock<IDocument> SetupContentStreamHash(this Mock<IDocument> doc, byte[] hash, string type = "SHA-1") {
            var hashString = string.Format("{{{0}}}{1}", type.ToLower(), BitConverter.ToString(hash).Replace("-", string.Empty));
            return doc.SetupContentStreamHash(hashString);
        }

        public static Mock<IDocument> SetupContentStreamHash(this Mock<IDocument> doc, string hashString) {
            var properties = new List<IProperty>();
            IList<object> values = new List<object>();
            values.Add(hashString);
            var property = Mock.Of<IProperty>(
                p =>
                p.IsMultiValued == true &&
                p.Id == "cmis:contentStreamHash" &&
                p.Values == values);

            properties.Add(property);
            doc.Setup(d => d.Properties).Returns(properties);
            return doc;
        }

        public static Mock<IDocument> SetupUpdateModificationDate(this Mock<IDocument> doc, DateTime? oldDate = null) {
            doc.Setup(d => d.LastModificationDate).Returns(oldDate);
            doc.Setup(d => d.UpdateProperties(
                It.Is<IDictionary<string, object>>(dic => dic.ContainsKey(PropertyIds.LastModificationDate)), true))
                .Callback<IDictionary<string, object>, bool>(
                    (dict, b) =>
                    doc.Setup(d => d.LastModificationDate).Returns((DateTime?)dict[PropertyIds.LastModificationDate]))
                .Returns(doc.Object);
            return doc;
        }

        public static Mock<IDocument> SetupCheckout(this Mock<IDocument> doc, Mock<IDocument> docPWC, string newChangeToken, string newObjectId = null) {
            doc.Setup(d => d.CheckOut()).Returns(() => {
                doc.Setup(d => d.IsVersionSeriesCheckedOut).Returns(true);
                doc.Setup(d => d.VersionSeriesCheckedOutId).Returns(docPWC.Object.Id);
                return Mock.Of<IObjectId>(o => o.Id == docPWC.Object.Id);
            });
            docPWC.Setup(d => d.CheckIn(It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IContentStream>(), It.IsAny<string>())).Returns(() => {
                doc.Setup(d => d.IsVersionSeriesCheckedOut).Returns(false);
                doc.Setup(d => d.VersionSeriesCheckedOutId).Returns(() => { return null; });
                doc.Setup(d => d.ChangeToken).Returns(newChangeToken);
                if (!string.IsNullOrEmpty(newObjectId)) {
                    doc.Setup(d => d.Id).Returns(newObjectId);
                    return Mock.Of<IObjectId>(o => o.Id == newObjectId);
                } else {
                    return Mock.Of<IObjectId>(o => o.Id == doc.Object.Id);
                }
            });
            return doc;
        }

        public static void VerifySetContentStream(this Mock<IDocument> doc, bool overwrite = true, bool refresh = true, string mimeType = null) {
            doc.VerifySetContentStream(Times.Once(), overwrite, refresh, mimeType);
        }

        public static void VerifySetContentStream(this Mock<IDocument> doc, Times times, bool overwrite = true, bool refresh = true, string mimeType = null) {
            doc.Verify(d => d.SetContentStream(It.Is<IContentStream>(s => VerifyContentStream(s, mimeType, doc.Object.Name)), overwrite, refresh), times);
        }

        public static void VerifyUpdateLastModificationDate(this Mock<IDocument> doc, DateTime modificationDate, bool refresh = true) {
            doc.VerifyUpdateLastModificationDate(modificationDate, Times.Once(), refresh);
        }

        public static void VerifyUpdateLastModificationDate(this Mock<IDocument> doc, DateTime modificationDate, Times times, bool refresh = true) {
            doc.Verify(d => d.UpdateProperties(It.Is<IDictionary<string, object>>(dic => VerifyDictContainsLastModification(dic, modificationDate)), refresh), times);
        }

        private static bool VerifyDictContainsLastModification(IDictionary<string, object> dic, DateTime modificationDate) {
            Assert.That(dic.ContainsKey(PropertyIds.LastModificationDate));
            Assert.That(dic[PropertyIds.LastModificationDate], Is.EqualTo(modificationDate));
            return true;
        }

        private static bool VerifyContentStream(IContentStream s, string mimeType, string fileName) {
            if (mimeType != null) {
                Assert.That(s.MimeType, Is.EqualTo(mimeType));
            }

            // Assert.That(s.Length, Is.Null);
            Assert.That(s.FileName, Is.EqualTo(fileName));
            return true;
        }
    }
}