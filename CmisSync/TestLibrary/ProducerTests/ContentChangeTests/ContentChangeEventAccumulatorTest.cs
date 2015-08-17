//-----------------------------------------------------------------------
// <copyright file="ContentChangeEventAccumulatorTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.ProducerTests.ContentChangeTests {
    using System;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Producer.ContentChange;
    using CmisSync.Lib.Producer.Watcher;
    using CmisSync.Lib.Queueing;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture, Category("Fast"), Category("ContentChange")]
    public class ContentChangeEventAccumulatorTest {
        private static readonly string Id = "myId";

        [Test]
        public void ConstructorTest() {
            var session = new Mock<ISession>(MockBehavior.Strict).Object;
            var queue = new Mock<ISyncEventQueue>(MockBehavior.Strict).Object;
            var underTest = new ContentChangeEventAccumulator(session, queue);
            Assert.That(underTest.Priority, Is.EqualTo(2000));
        }

        [Test]
        public void DbNullContstructorTest() {
            Assert.Throws<ArgumentNullException>(() => new ContentChangeEventAccumulator(null, Mock.Of<ISyncEventQueue>()));
        }

        [Test]
        public void DocumentCreationAccumulated() {
            var session = new Mock<ISession>(MockBehavior.Strict).SetupCreateOperationContext();
            var remoteObject = new Mock<ICmisObject>();
            session.Setup(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>())).Returns(remoteObject.Object);
            var contentChange = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Created, Id);
            var underTest = new ContentChangeEventAccumulator(session.Object, Mock.Of<ISyncEventQueue>());

            Assert.That(underTest.Handle(contentChange), Is.False);

            Assert.That(contentChange.CmisObject, Is.EqualTo(remoteObject.Object));
        }

        [Test]
        public void DocumentAlreadyAccumulatedIsNotAccumulatedAgain() {
            var session = new Mock<ISession>(MockBehavior.Strict).SetupCreateOperationContext();
            var remoteObject = Mock.Of<ICmisObject>();
            var newRemoteObject = Mock.Of<ICmisObject>();
            session.Setup(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>())).Returns(remoteObject);
            var contentChange = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Created, Id);
            contentChange.UpdateObject(session.Object);
            session.Setup(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>())).Returns(newRemoteObject);
            var unterTest = new ContentChangeEventAccumulator(session.Object, Mock.Of<ISyncEventQueue>());

            Assert.That(unterTest.Handle(contentChange), Is.False);

            Assert.That(contentChange.CmisObject, Is.EqualTo(remoteObject));
            Assert.That(contentChange.CmisObject, Is.Not.EqualTo(newRemoteObject));
        }

        [Test]
        public void DocumentDeletionNotAccumulated() {
            var session = new Mock<ISession>(MockBehavior.Strict).Object;
            var contentChange = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Deleted, Id);
            var underTest = new ContentChangeEventAccumulator(session, Mock.Of<ISyncEventQueue>());

            Assert.That(underTest.Handle(contentChange), Is.False);

            Assert.That(contentChange.CmisObject, Is.Null);
        }

        [Test]
        public void DoesNotHandleWrongEvents() {
            var session = new Mock<ISession>(MockBehavior.Strict);
            var wrongEvent = new Mock<ISyncEvent>(MockBehavior.Strict).Object;

            var underTest = new ContentChangeEventAccumulator(session.Object, Mock.Of<ISyncEventQueue>());

            Assert.That(underTest.Handle(wrongEvent), Is.False);
        }

        [Test]
        public void IgnoreEventsThatHaveBeenDeleted() {
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>(MockBehavior.Strict).SetupCreateOperationContext();
            session.Setup(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>())).Throws<CmisObjectNotFoundException>();
            var underTest = new ContentChangeEventAccumulator(session.Object, queue.Object);
            var contentChange = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Created, Id);

            Assert.That(underTest.Handle(contentChange), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<StartNextSyncEvent>()), Times.Never());
        }

        [Test]
        public void IgnoreEventsThatWeDontHaveAccessTo() {
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>(MockBehavior.Strict).SetupCreateOperationContext();
            session.Setup(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>())).Throws<CmisPermissionDeniedException>();
            var underTest = new ContentChangeEventAccumulator(session.Object, queue.Object);
            var contentChange = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Created, Id);

            Assert.That(underTest.Handle(contentChange), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<StartNextSyncEvent>()), Times.Never());
        }

        [Test]
        public void ExceptionTriggersFullSync() {
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>(MockBehavior.Strict);
            session.Setup(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>())).Throws<Exception>();
            var underTest = new ContentChangeEventAccumulator(session.Object, queue.Object);
            var contentChange = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Created, Id);

            Assert.That(underTest.Handle(contentChange), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<StartNextSyncEvent>()), Times.Once());
        }
    }
}