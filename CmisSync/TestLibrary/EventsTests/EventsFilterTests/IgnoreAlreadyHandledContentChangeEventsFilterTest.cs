//-----------------------------------------------------------------------
// <copyright file="IgnoreAlreadyHandledContentChangeEventsFilterTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.EventsTests.EventsFilterTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Events.Filter;
    using CmisSync.Lib.Storage;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class IgnoreAlreadyHandledContentChangeEventsFilterTest
    {
        [Test, Category("Fast")]
        public void ConstructorTakesStorageAndSeesionInstance()
        {
            new IgnoreAlreadyHandledContentChangeEventsFilter(Mock.Of<IMetaDataStorage>(), Mock.Of<ISession>());
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionOnNullStorage()
        {
            new IgnoreAlreadyHandledContentChangeEventsFilter(null, Mock.Of<ISession>());
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionOnNullSession()
        {
            new IgnoreAlreadyHandledContentChangeEventsFilter(Mock.Of<IMetaDataStorage>(), null);
        }

        [Test, Category("Fast")]
        public void PriorityIsFilterPriority()
        {
            var filter = new IgnoreAlreadyHandledContentChangeEventsFilter(Mock.Of<IMetaDataStorage>(), Mock.Of<ISession>());
            Assert.That(filter.Priority, Is.EqualTo(EventHandlerPriorities.FILTER));
        }

        [Test, Category("Fast")]
        public void DoesNotHandleUnknownEvents()
        {
            var filter = new IgnoreAlreadyHandledContentChangeEventsFilter(Mock.Of<IMetaDataStorage>(), Mock.Of<ISession>());

            Assert.That(filter.Handle(Mock.Of<ISyncEvent>()), Is.False);
        }

        [Test, Category("Fast")]
        public void DoesNotFilterAddEventsOfNotExistingObjects()
        {
            string remoteId = "remoteId";
            var filter = new IgnoreAlreadyHandledContentChangeEventsFilter(Mock.Of<IMetaDataStorage>(), Mock.Of<ISession>());
            var contentChangeEvent = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Created, remoteId);

            Assert.That(filter.Handle(contentChangeEvent), Is.False);
        }

        [Test, Category("Fast")]
        public void DoesNotFilterChangeEventsOfNotExistingObjects()
        {
            string remoteId = "remoteId";
            var filter = new IgnoreAlreadyHandledContentChangeEventsFilter(Mock.Of<IMetaDataStorage>(), Mock.Of<ISession>());
            var contentChangeEvent = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Updated, remoteId);

            Assert.That(filter.Handle(contentChangeEvent), Is.False);
        }

        [Test, Category("Fast")]
        public void DoesNotFilterChangeEventsOfExistingButDifferentObjects()
        {
            string remoteId = "remoteId";
            string oldToken = "oldToken";
            string newToken = "newToken";
            var session = new Mock<ISession>();
            var storage = new Mock<IMetaDataStorage>();
            var filter = new IgnoreAlreadyHandledContentChangeEventsFilter(storage.Object, session.Object);
            var mappedObject = Mock.Of<IMappedObject>(
                o =>
                o.LastChangeToken == oldToken &&
                o.RemoteObjectId == remoteId);
            storage.Setup(s => s.GetObjectByRemoteId(It.Is<string>(id => id == remoteId))).Returns(mappedObject);
            var remoteObject = Mock.Of<ICmisObject>(
                o =>
                o.ChangeToken == newToken);
            session.Setup(s => s.GetObject(It.Is<string>(id => id == remoteId), It.IsAny<IOperationContext>())).Returns(remoteObject);
            var contentChangeEvent = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Updated, remoteId);

            Assert.That(filter.Handle(contentChangeEvent), Is.False);
        }

        [Test, Category("Fast")]
        public void FilterHandlesDeletedEventsOfNonLocalExistingObjects()
        {
            var filter = new IgnoreAlreadyHandledContentChangeEventsFilter(Mock.Of<IMetaDataStorage>(), Mock.Of<ISession>());
            var contentChangeEvent = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Deleted, "remoteId");

            Assert.That(filter.Handle(contentChangeEvent), Is.True);
        }

        [Test, Category("Fast")]
        public void FilterIgnoresDeletedEventsOfLocalExistingObjects()
        {
            string remoteId = "remoteId";
            var storage = new Mock<IMetaDataStorage>();
            storage.Setup(s => s.GetObjectByRemoteId(It.Is<string>(id => id == remoteId))).Returns(Mock.Of<IMappedObject>());
            var filter = new IgnoreAlreadyHandledContentChangeEventsFilter(storage.Object, Mock.Of<ISession>());
            var contentChangeEvent = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Deleted, remoteId);

            Assert.That(filter.Handle(contentChangeEvent), Is.False);
        }

        [Test, Category("Fast")]
        public void FiltersChangeEventsIfChangeLogTokenIsEqualToLocalObject()
        {
            string remoteId = "remoteId";
            string changeToken = "Token";
            string parentId = "parentId";
            var session = new Mock<ISession>();
            var storage = new Mock<IMetaDataStorage>();
            var filter = new IgnoreAlreadyHandledContentChangeEventsFilter(storage.Object, session.Object);
            var mappedObject = Mock.Of<IMappedObject>(
                o =>
                o.LastChangeToken == changeToken &&
                o.RemoteObjectId == remoteId &&
                o.ParentId == parentId);
            storage.Setup(s => s.GetObjectByRemoteId(It.Is<string>(id => id == remoteId))).Returns(mappedObject);
            var remoteObject = Mock.Of<IFolder>(
                o =>
                o.ChangeToken == changeToken &&
                o.ParentId == parentId);
            session.Setup(s => s.GetObject(It.Is<string>(id => id == remoteId), It.IsAny<IOperationContext>())).Returns(remoteObject);
            var contentChangeEvent = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Updated, remoteId);

            Assert.That(filter.Handle(contentChangeEvent), Is.True);
        }

        [Test, Category("Fast")]
        public void FilterIgnoresFolderChangedEventsIfChangeLogTokenIsEqualButParentIdIsDifferent()
        {
            string remoteId = "remoteId";
            string changeToken = "Token";
            var session = new Mock<ISession>();
            var storage = new Mock<IMetaDataStorage>();
            var filter = new IgnoreAlreadyHandledContentChangeEventsFilter(storage.Object, session.Object);
            var mappedObject = Mock.Of<IMappedObject>(
                o =>
                o.LastChangeToken == changeToken &&
                o.RemoteObjectId == remoteId &&
                o.Type == MappedObjectType.Folder &&
                o.ParentId == "oldParent");
            storage.Setup(s => s.GetObjectByRemoteId(It.Is<string>(id => id == remoteId))).Returns(mappedObject);
            var remoteFolder = Mock.Of<IFolder>(
                o =>
                o.ChangeToken == changeToken &&
                o.ParentId == "newParent");
            session.Setup(s => s.GetObject(It.Is<string>(id => id == remoteId), It.IsAny<IOperationContext>())).Returns(remoteFolder);
            var contentChangeEvent = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Updated, remoteId);

            Assert.That(filter.Handle(contentChangeEvent), Is.False);
        }

        [Test, Category("Fast")]
        public void FilterIgnoresFileChangedEventsIfChangeLogTokenIsEqualButParentIdIsDifferent()
        {
            string remoteId = "remoteId";
            string changeToken = "Token";
            var session = new Mock<ISession>();
            var storage = new Mock<IMetaDataStorage>();
            var filter = new IgnoreAlreadyHandledContentChangeEventsFilter(storage.Object, session.Object);
            var mappedObject = Mock.Of<IMappedObject>(
                o =>
                o.LastChangeToken == changeToken &&
                o.RemoteObjectId == remoteId &&
                o.ParentId == "oldParent");
            storage.Setup(s => s.GetObjectByRemoteId(It.Is<string>(id => id == remoteId))).Returns(mappedObject);
            var remoteDocument = Mock.Of<IDocument>(
                o =>
                o.ChangeToken == changeToken);
            Mock.Get(remoteDocument).SetupParent(Mock.Of<IFolder>(f => f.Id == "newParent"));
            session.Setup(s => s.GetObject(It.Is<string>(id => id == remoteId), It.IsAny<IOperationContext>())).Returns(remoteDocument);
            var contentChangeEvent = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Updated, remoteId);

            Assert.That(filter.Handle(contentChangeEvent), Is.False);
        }
    }
}