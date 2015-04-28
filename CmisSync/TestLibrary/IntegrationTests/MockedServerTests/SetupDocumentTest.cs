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
            if (content == null) {
                Assert.That(underTest.ContentStreamLength, Is.Null.Or.EqualTo(0));
            } else {
                Assert.That(underTest.ContentStreamLength, Is.EqualTo(content.Length));
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
    }
}