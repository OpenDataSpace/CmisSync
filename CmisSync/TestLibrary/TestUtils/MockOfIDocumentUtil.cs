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
using DotCMIS;

namespace TestLibrary.TestUtils
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using DotCMIS.Client;
    using DotCMIS.Data;

    using Moq;

    using NUnit.Framework;

    public static class MockOfIDocumentUtil
    {
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
            newRemoteObject.Setup(d => d.ChangeToken).Returns(changeToken);
            newRemoteObject.SetupContent(content, name);
            newRemoteObject.SetupParent(parent);
            return newRemoteObject;
        }

        public static void SetupContent(this Mock<IDocument> doc, byte[] content, string fileName, string mimeType = "application/octet-stream") {
            if(content != null) {
                var stream = Mock.Of<IContentStream>(
                    s =>
                    s.Length == content.Length &&
                    s.MimeType == mimeType &&
                    s.FileName == fileName &&
                    s.Stream == new MemoryStream(content));
                doc.Setup(d => d.GetContentStream()).Returns(stream);
            }
        }

        public static void SetupParent(this Mock<IDocument> doc, IFolder parent) {
            List<IFolder> parents = new List<IFolder>();
            parents.Add(parent);
            doc.Setup(d => d.Parents).Returns(parents);
        }

        public static void SetupPath(this Mock<IDocument> doc, params string[] path) {
            List<string> paths = new List<string>(path);
            doc.Setup(d => d.Paths).Returns(paths);
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
            doc.Verify(d => d.UpdateProperties(It.Is<IDictionary<string, object>>(dic => VerifyDictContainsLastModification(dic, modificationDate)), refresh));
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