//-----------------------------------------------------------------------
// <copyright file="ContentChangeIT.cs" company="GRAU DATA AG">
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

namespace TestLibrary.IntegrationTests {
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Config;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Producer.ContentChange;
    using CmisSync.Lib.Producer.Watcher;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Binding.Services;
    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Data.Extensions;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class ContentChangeIT : IsTestWithConfiguredLog4Net {
        private static readonly string DefaultId = "defaultId";

        private readonly bool isPropertyChangesSupported = false;
        private readonly int maxNumberOfContentChanges = 1000;

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteSecurityChangeOfExistingFile() {
            Mock<IMetaDataStorage> storage = MockMetaDataStorageUtil.GetMetaStorageMockWithToken();
            var path = Mock.Of<IFileInfo>(f => f.FullName == "path");
            storage.AddLocalFile(path, DefaultId);

            Mock<ISession> session = MockSessionUtil.GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Security, DefaultId);
            ObservableHandler observer = this.RunQueue(session, storage);

            storage.Verify(s => s.GetObjectByRemoteId(DefaultId), Times.Once());

            observer.AssertGotSingleFileEvent(MetaDataChangeType.CHANGED, ContentChangeType.NONE);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteDocumentCreationWithContent() {
            Mock<IMetaDataStorage> storage = MockMetaDataStorageUtil.GetMetaStorageMockWithToken();

            Mock<ISession> session = MockSessionUtil.GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Created, DefaultId, "someStreamId");

            ObservableHandler observer = this.RunQueue(session, storage);

            observer.AssertGotSingleFileEvent(MetaDataChangeType.CREATED, ContentChangeType.CREATED);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void LocallyNotExistingRemoteDocumentUpdated() {
            Mock<IMetaDataStorage> storage = MockMetaDataStorageUtil.GetMetaStorageMockWithToken();

            Mock<ISession> session = MockSessionUtil.GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Updated, DefaultId, null);

            ObservableHandler observer = this.RunQueue(session, storage);

            observer.AssertGotSingleFileEvent(MetaDataChangeType.CREATED, ContentChangeType.CREATED);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteDeletionChangeTest() {
            Mock<IMetaDataStorage> storage = MockMetaDataStorageUtil.GetMetaStorageMockWithToken();
            var file = Mock.Of<IFileInfo>(f => f.FullName == "path");
            storage.AddLocalFile(file, DefaultId);

            Mock<ISession> session = MockSessionUtil.GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Deleted, DefaultId, null);
            ObservableHandler observer = this.RunQueue(session, storage);

            observer.AssertGotSingleFileEvent(MetaDataChangeType.DELETED, ContentChangeType.NONE);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteFolderCreation() {
            Mock<IMetaDataStorage> storage = MockMetaDataStorageUtil.GetMetaStorageMockWithToken();

            Mock<ISession> session = MockSessionUtil.GetSessionMockReturningFolderChange(DotCMIS.Enums.ChangeType.Created);
            ObservableHandler observer = this.RunQueue(session, storage);

            observer.AssertGotSingleFolderEvent(MetaDataChangeType.CREATED);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteFolderDeletionWithoutLocalFolder() {
            Mock<IMetaDataStorage> storage = MockMetaDataStorageUtil.GetMetaStorageMockWithToken();

            Mock<ISession> session = MockSessionUtil.GetSessionMockReturningFolderChange(DotCMIS.Enums.ChangeType.Deleted);
            ObservableHandler observer = this.RunQueue(session, storage);
            Assert.That(observer.List.Count, Is.EqualTo(0));
        }

        private ObservableHandler RunQueue(Mock<ISession> session, Mock<IMetaDataStorage> storage) {
            var manager = new SyncEventManager();

            var observer = new ObservableHandler();
            manager.AddEventHandler(observer);

            SingleStepEventQueue queue = new SingleStepEventQueue(manager);

            var changes = new ContentChanges(session.Object, storage.Object, queue, this.maxNumberOfContentChanges, this.isPropertyChangesSupported);
            manager.AddEventHandler(changes);

            var transformer = new ContentChangeEventTransformer(queue, storage.Object);
            manager.AddEventHandler(transformer);

            var accumulator = new ContentChangeEventAccumulator(session.Object, queue);
            manager.AddEventHandler(accumulator);

            queue.RunStartSyncEvent();

            return observer;
        }
    }
}