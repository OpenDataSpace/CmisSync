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

namespace TestLibrary.SyncStrategiesTests
{
    using System;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Sync.Strategy;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ContentChangeEventAccumulatorTest 
    {
        private static readonly string Id = "myId";

        [Test, Category("Fast"), Category("ContentChange")]
        public void ConstructorTest() {
            var session = new Mock<ISession>();
            var accumulator = new ContentChangeEventAccumulator(session.Object, new Mock<ISyncEventQueue>().Object);
            Assert.That(accumulator.Priority, Is.EqualTo(2000));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DbNullContstructorTest() {
            new ContentChangeEventAccumulator(null, new Mock<ISyncEventQueue>().Object);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void DocumentCreationAccumulated() {
            var session = new Mock<ISession>();
            var remoteObject = new Mock<ICmisObject>();
            session.Setup(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>())).Returns(remoteObject.Object);
            var accumulator = new ContentChangeEventAccumulator(session.Object, new Mock<ISyncEventQueue>().Object);
            var contentChange = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Created, Id);

            Assert.That(accumulator.Handle(contentChange), Is.False);
            Assert.That(contentChange.CmisObject, Is.EqualTo(remoteObject.Object));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void DocumentAlreadyAccumulatedIsNotAccumulatedAgain() {
            var session = new Mock<ISession>();
            var remoteObject = Mock.Of<ICmisObject>();
            var newRemoteObject = Mock.Of<ICmisObject>();
            session.Setup(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>())).Returns(remoteObject);
            var accumulator = new ContentChangeEventAccumulator(session.Object, new Mock<ISyncEventQueue>().Object);
            var contentChange = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Created, Id);
            contentChange.UpdateObject(session.Object);
            session.Setup(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>())).Returns(newRemoteObject);

            Assert.That(accumulator.Handle(contentChange), Is.False);
            Assert.That(contentChange.CmisObject, Is.EqualTo(remoteObject));
            Assert.That(contentChange.CmisObject, Is.Not.EqualTo(newRemoteObject));
        }


        [Test, Category("Fast"), Category("ContentChange")]
        public void DocumentDeletionNotAccumulated() {
            var session = new Mock<ISession>();
            var contentChange = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Deleted, Id);
            var accumulator = new ContentChangeEventAccumulator(session.Object, new Mock<ISyncEventQueue>().Object);

            accumulator.Handle(contentChange);
            Assert.That(contentChange.CmisObject, Is.Null);
            session.Verify(foo => foo.GetObject(It.IsAny<string>()), Times.Never());
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void DoesNotHandleWrongEvents() {
            var session = new Mock<ISession>();
            var contentChange = new Mock<ISyncEvent>().Object;

            var accumulator = new ContentChangeEventAccumulator(session.Object, new Mock<ISyncEventQueue>().Object);
            Assert.That(accumulator.Handle(contentChange), Is.False);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void IgnoreEventsThatHaveBeenDeleted() {
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            session.Setup(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>())).Throws(new CmisObjectNotFoundException());
            var accumulator = new ContentChangeEventAccumulator(session.Object, queue.Object);
            var contentChange = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Created, Id);

            Assert.That(accumulator.Handle(contentChange), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<StartNextSyncEvent>()), Times.Never());
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void IgnoreEventsThatWeDontHaveAccessTo() {
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            session.Setup(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>())).Throws(new CmisPermissionDeniedException());
            var accumulator = new ContentChangeEventAccumulator(session.Object, queue.Object);
            var contentChange = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Created, Id);

            Assert.That(accumulator.Handle(contentChange), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<StartNextSyncEvent>()), Times.Never());
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void ExceptionTriggersFullSync() {
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            session.Setup(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>())).Throws(new Exception());
            var accumulator = new ContentChangeEventAccumulator(session.Object, queue.Object);
            var contentChange = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Created, Id);

            Assert.That(accumulator.Handle(contentChange), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<StartNextSyncEvent>()), Times.Once());
        }
    }
}
