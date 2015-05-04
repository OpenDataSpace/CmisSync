//-----------------------------------------------------------------------
// <copyright file="SetupDocumentTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.IntegrationTests.MockedServerTests {
    using System;
    using System.IO;
    using System.Text;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS.Client;
    using DotCMIS.Enums;

    using MockedServer;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class SetupDocumentTest {
        [Test, Category("Fast")]
        public void CreateMockedDocument([Values(null, "content")]string content) {
            var parent = Mock.Of<IFolder>();
            var name = "name";
            var id = "id";

            var underTest = new MockedDocument(name, content, id, parent).Object;

            Assert.That(underTest.Name, Is.EqualTo(name));
            Assert.That(underTest.Id, Is.EqualTo(id));
            Assert.That(underTest.Parents, Is.All.EqualTo(parent));
            Assert.That(underTest.CreatedBy, Is.Null.Or.Not.Null);
            Assert.That(underTest.LastModificationDate, Is.Null.Or.Not.Null);
            Assert.That(underTest.ContentStreamId, Is.Null);
            var contentStream = underTest.GetContentStream();
            if (content == null) {
                Assert.That(underTest.ContentStreamLength, Is.Null.Or.EqualTo(0));
                Assert.That(contentStream, Is.Null);
            } else {
                Assert.That(underTest.ContentStreamLength, Is.EqualTo(content.Length));
                Assert.That(contentStream, Is.Not.Null);
                Assert.That(contentStream.FileName, Is.EqualTo(name));
                Assert.That(contentStream.MimeType, Is.Not.Null);
                Assert.That(contentStream.Length, Is.EqualTo(underTest.ContentStreamLength));
                using (var stream = contentStream.Stream) {
                    Assert.That(stream.Length, Is.EqualTo(underTest.ContentStreamLength));
                }
            }
        }

        [Test, Category("Fast")]
        public void ContentStreamRegion() {
            var content = "AB";
            var underTest = new MockedDocument("name", content, "id").Object;
            var firstPart = underTest.GetContentStream(null, 0, 1);
            var secondPart = underTest.GetContentStream(null, 1, 1);
            var fullContent = underTest.GetContentStream();
            Assert.That(firstPart.Length, Is.EqualTo(1));
            Assert.That(firstPart.FileName, Is.EqualTo(underTest.Name));
            Assert.That(secondPart.Length, Is.EqualTo(1));
            Assert.That(secondPart.FileName, Is.EqualTo(underTest.Name));
            Assert.That(fullContent.Length, Is.EqualTo(content.Length));
            using (var concatStream = new MemoryStream())
            using (var firstStream = firstPart.Stream)
            using (var secondStream = secondPart.Stream) {
                firstStream.CopyTo(concatStream);
                secondStream.CopyTo(concatStream);
                Assert.That(concatStream.ToArray(), Is.EqualTo(Encoding.UTF8.GetBytes(content)));
            }
        }

        [Test, Category("Fast")]
        public void MockedDocumentRaisesEventOnUpdateProperties() {
            var contentChangeHandler = new Mock<ContentChangeEventHandler>();
            var newDate = DateTime.UtcNow.AddHours(1);
            var underTest = new MockedDocument("name");
            var oldChangeToken = underTest.Object.ChangeToken;
            underTest.ContentChanged += contentChangeHandler.Object;

            underTest.Object.UpdateLastWriteTimeUtc(newDate);

            Assert.That(underTest.Object.LastModificationDate, Is.EqualTo(newDate).Within(1).Seconds);
            Assert.That(underTest.Object.ChangeToken, Is.Not.EqualTo(oldChangeToken));
            contentChangeHandler.Verify(h => h(underTest, It.Is<IChangeEvent>(e => e.ChangeType == ChangeType.Updated && e.ObjectId == underTest.Object.Id)), Times.Once());
        }

        [Test, Category("Fast")]
        public void RenamingDocumentRaisesEvent([Values(true, false, null)]bool? refresh) {
            var contentChangeHandler = new Mock<ContentChangeEventHandler>();
            var newName = "new name";
            var underTest = new MockedDocument("name");
            var oldChangeToken = underTest.Object.ChangeToken;
            underTest.ContentChanged += contentChangeHandler.Object;

            if (refresh == null) {
                underTest.Object.Rename(newName);
            } else {
                underTest.Object.Rename(newName, (bool)refresh);
            }

            Assert.That(underTest.Name, Is.EqualTo(newName));
            Assert.That(underTest.Object.ChangeToken, Is.Not.EqualTo(oldChangeToken));
            contentChangeHandler.Verify(h => h(underTest, It.Is<IChangeEvent>(e => e.ChangeType == ChangeType.Updated && e.ObjectId == underTest.Object.Id)), Times.Once());
        }

        [Test, Category("Fast")]
        public void DeleteContentStream([Values(true, false, null)]bool? refresh, [Values("content", null)]string content) {
            var contentChangeHandler = new Mock<ContentChangeEventHandler>();
            var underTest = new MockedDocument("name", content);
            var oldChangeToken = underTest.Object.ChangeToken;
            underTest.ContentChanged += contentChangeHandler.Object;

            if (refresh == null) {
                Assert.That(underTest.Object.DeleteContentStream().Id, Is.EqualTo(underTest.Id));
            } else {
                Assert.That(underTest.Object.DeleteContentStream((bool)refresh).Id, Is.EqualTo(underTest.Id));
            }

            Assert.That(underTest.Object.ContentStreamLength, Is.Null.Or.EqualTo(0));
            Assert.That(underTest.Object.GetContentStream(), Is.Null);
            if (content != null) {
                Assert.That(underTest.Object.ChangeToken, Is.Not.EqualTo(oldChangeToken));
            } else {
                Assert.That(underTest.Object.ChangeToken, Is.EqualTo(oldChangeToken));
            }

            contentChangeHandler.Verify(h => h(underTest, It.Is<IChangeEvent>(e => e.ChangeType == ChangeType.Updated && e.ObjectId == underTest.Object.Id)), content == null ? Times.Never() : Times.Once());
        }

        [Test, Category("Fast")]
        public void DeleteObject([Values(true, false)]bool allVersions, [Values(true, false)]bool withSession) {
            var session = new MockedSession(new MockedRepository().Object);
            var contentChangeHandler = new Mock<ContentChangeEventHandler>();
            var underTest = new MockedDocument("name", "content") { MockedSession = withSession ? session : null };
            session.Objects.Add(underTest.Object.Id, underTest.Object);
            underTest.ContentChanged += contentChangeHandler.Object;

            underTest.Object.Delete(allVersions);

            Assert.That(session.Objects, withSession ? Is.Empty : Is.Not.Empty);
            contentChangeHandler.Verify(h => h(underTest, It.Is<IChangeEvent>(e => e.ChangeType == ChangeType.Deleted && e.ObjectId == underTest.Object.Id)), Times.Once());
        }
    }
}