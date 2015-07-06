//-----------------------------------------------------------------------
// <copyright file="MockedDocument.cs" company="GRAU DATA AG">
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

namespace TestLibrary.MockedServer {
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Streams;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Enums;
    using DotCMIS.Exceptions;

    using Moq;

    using TestUtils;

    public class MockedDocument : MockedCmisObject<IDocument> {
        private IDocumentType docType = new MockedDocumentType().Object;
        private IFolder parent;
        private string defaultStreamId = null;

        public MockedDocument(
            string name,
            string content = null,
            string id = null,
            IFolder parent = null,
            MockBehavior behavior = MockBehavior.Strict) : base(name, id, behavior) {
            this.ObjectType = this.docType;
            this.Renditions = new List<IRendition>();
            this.parent = parent ?? Mock.Of<IFolder>();
            this.Setup(m => m.BaseType).Returns(this.docType);
            this.Setup(m => m.BaseTypeId).Returns(BaseTypeId.CmisDocument);
            this.SetupParent(this.parent);
            this.Setup(m => m.ContentStreamId).Returns(defaultStreamId);
            this.Setup(m => m.ContentStreamLength).Returns(() => this.Stream == null ? (long?)null : this.Stream.Length);
            this.Setup(m => m.ContentStreamFileName).Returns(() => this.Stream != null ? this.Stream.FileName : name);
            this.Setup(m => m.SetContentStream(It.Is<IContentStream>(s => s != null), It.IsAny<bool>())).Callback<IContentStream, bool>((stream, overwrite) => this.SetContent(stream, overwrite)).Returns(() => this.Object);
            this.Setup(m => m.SetContentStream(It.Is<IContentStream>(s => s != null), It.IsAny<bool>(), It.IsAny<bool>())).Callback<IContentStream, bool, bool>((stream, o, r) => this.SetContent(stream, o)).Returns(() => Mock.Of<IObjectId>(oid => oid.Id == this.Object.Id));
            this.Setup(m => m.GetContentStream()).Returns(() => this.Stream);
            this.Setup(m => m.GetContentStream(It.Is<string>(sid => sid == null || sid.Equals(this.defaultStreamId)), It.IsAny<long>(), It.IsAny<long>())).Returns<string, long?, long?>((sid, offset, length) => {
                var subStream = new MockedContentStream(this.Stream.FileName, behavior: behavior);
                using (var orig = this.Stream.Stream)
                using (var part = new MemoryStream()) {
                    orig.Seek(offset.GetValueOrDefault(), SeekOrigin.Begin);
                    orig.CopyTo(part, 8192, (int)length.GetValueOrDefault());
                    subStream.Content = part.ToArray();
                }

                return subStream.Object;
            });
            if (content != null) {
                this.Stream = new MockedContentStream(name, content, behavior: behavior).Object;
            }

            this.Setup(m => m.Renditions).Returns(() => new List<IRendition>(this.Renditions));
            this.Setup(m => m.IsPrivateWorkingCopy).Returns(() => this.IsPrivateWorkingCopy);
            this.Setup(m => m.DeleteContentStream()).Callback(() => { if (this.Stream != null) {this.Stream = null; this.UpdateChangeToken(); this.NotifyChanges(); }}).Returns(() => this.Object);
            this.Setup(m => m.DeleteContentStream(It.IsAny<bool>())).Callback(() => { if (this.Stream != null) {this.Stream = null; this.UpdateChangeToken(); this.NotifyChanges(); }}).Returns(() => Mock.Of<IObjectId>(oid => oid.Id == this.Object.Id));
            this.Setup(m => m.Delete(It.IsAny<bool>())).Callback<bool>((allVersions) => this.Delete(allVersions));
            this.Setup(m => m.DeleteAllVersions()).Callback(() => this.Delete(true));
            this.Setup(m => m.GetAllVersions()).Returns(() => new List<IDocument>(this.AllVersions));
            this.AllVersions = new List<IDocument>(new IDocument[]{ this.Object });
        }

        public IContentStream Stream { get; set; }

        public IList<IRendition> Renditions { get; set; }

        public bool IsPrivateWorkingCopy { get; set; }

        public MockedSession MockedSession { get; set; }

        public List<IDocument> AllVersions { get; set; }

        private void Delete(bool allVersions) {
            if (allVersions) {
                foreach (var doc in this.Object.GetAllVersions()) {
                    doc.Delete(false);
                }
            } else {
                if (this.MockedSession != null) {
                    this.MockedSession.Objects.Remove(this.Object.Id);
                    this.AllVersions.Remove(this.Object);
                }

                this.NotifyChanges(ChangeType.Deleted);
            }
        }

        private void SetContent(IContentStream inputstream, bool overwrite) {
            if (this.Stream != null && this.Stream.Length != null && this.Stream.Length > 0 && !overwrite) {
                throw new CmisContentAlreadyExistsException();
            }

            var newContent = new MockedContentStream(inputstream.FileName, null, inputstream.MimeType);
            using (var memStream = new MemoryStream())
            using (var input = inputstream.Stream){
                input.CopyTo(memStream);
                newContent.Content = memStream.ToArray();
            }

            this.Stream = newContent.Object;
            this.UpdateChangeToken();
            this.NotifyChanges();
        }
    }
}