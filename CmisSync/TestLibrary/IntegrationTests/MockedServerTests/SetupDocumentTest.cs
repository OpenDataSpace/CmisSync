using DotCMIS.Client;
using DotCMIS.Enums;


namespace TestLibrary.IntegrationTests.MockedServerTests {
    using System;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

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
            var underTest = new MockedDocument("name");
            var contentChangeHandler = new Mock<ContentChangeEventHandler>();
            var newDate = DateTime.UtcNow.AddHours(1);
            var oldChangeToken = underTest.Object.ChangeToken;
            underTest.ContentChanged += contentChangeHandler.Object;

            underTest.Object.UpdateLastWriteTimeUtc(newDate);

            Assert.That(underTest.Object.LastModificationDate, Is.EqualTo(newDate).Within(1).Seconds);
            Assert.That(underTest.Object.ChangeToken, Is.Not.EqualTo(oldChangeToken));
            contentChangeHandler.Verify(h => h(underTest, It.Is<IChangeEvent>(e => e.ChangeType == ChangeType.Updated && e.ObjectId == underTest.Object.Id)), Times.Once());
        }
    }
}