//-----------------------------------------------------------------------
// <copyright file="SuccessfulLoginHandlerTest.cs" company="GRAU DATA AG">
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
namespace TestLibrary.EventsTests
{
    using System;
    using System.IO;

    using CmisSync.Lib;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class SuccessfulLoginHandlerTest
    {
        private string remoteRoot = "/my/";
        
        private RepoInfo CreateRepoInfo() {
            return new RepoInfo
            {
                Address = new Uri("http://example.com"),
                LocalPath = Path.GetTempPath(),
                RemotePath = "/"
            };
        }
        
        //ISyncEventQueue queue, IMetaDataStorage storage, SyncEventManager manager, RepoInfo repoInfo, IFileSystemInfoFactory fsFactory = null
        
        [Test, Category("Fast")]
        public void ConstructorTest()
        {
            var queue = new Mock<ISyncEventQueue>();
            var manager = new Mock<SyncEventManager>();
            var storage = new Mock<IMetaDataStorage>();
            new SuccessfulLoginHandler(queue.Object, storage.Object, manager.Object, CreateRepoInfo());
        }
        
        [Test, Category("Fast")]
        public void IgnoresWrongEventsTest()
        {
            var queue = new Mock<ISyncEventQueue>();
            var manager = new Mock<SyncEventManager>();
            var storage = new Mock<IMetaDataStorage>();
            var handler = new SuccessfulLoginHandler(queue.Object, storage.Object, manager.Object, CreateRepoInfo());
            
            var e = new Mock<ISyncEvent>();
            Assert.False(handler.Handle(e.Object));            
        }
        
        [Test, Category("Fast")]
        public void RootFolderGetsAddedToStorage()
        {
            string id = "id";
            string token = "token";
            var session = new Mock<ISession>();
            var remoteObject = new Mock<IFolder>();
            remoteObject.Setup(r => r.Id).Returns(id);
            remoteObject.Setup(r => r.ChangeToken).Returns(token);

            session.Setup(s => s.GetObjectByPath(It.IsAny<string>())).Returns(remoteObject.Object);
            var queue = new Mock<ISyncEventQueue>();
            var manager = new Mock<SyncEventManager>();
            var storage = new Mock<IMetaDataStorage>();
            var handler = new SuccessfulLoginHandler(queue.Object, storage.Object, manager.Object, CreateRepoInfo());
            
            var e = new SuccessfulLoginEvent(new Uri("http://example.com"), session.Object);
            Assert.True(handler.Handle(e));
            MappedObject rootObject = new MappedObject("/", id, MappedObjectType.Folder, null, token);
            storage.Verify(s => s.SaveMappedObject(It.Is<MappedObject>(m => m.Equals(rootObject))));
        }
    }
}
