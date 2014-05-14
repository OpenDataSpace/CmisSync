//-----------------------------------------------------------------------
// <copyright file="CmisRepoTest.cs" company="GRAU DATA AG">
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
namespace TestLibrary.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    using CmisSync.Lib;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync;

    using DBreeze;

    using DotCMIS.Binding;
    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class CmisRepoTest
    {
        [TestFixtureSetUp]
        public void ClassInit()
        {
            log4net.Config.XmlConfigurator.Configure(ConfigManager.CurrentConfig.GetLog4NetConfig());
        }

        [Test, Category("Fast")]
        public void CmisRepoCanBeConstructed() {
            string path = Path.GetTempPath();
            RepoInfo repoInfo = this.CreateRepoInfo(path);
            var activityListener = new Mock<IActivityListener>().Object;
            var session = new Mock<ISession>();
            new CmisRepoWrapper(repoInfo, activityListener, session.Object, new SingleStepEventQueue(new Mock<ISyncEventManager>().Object));
        }

        [Test, Category("Fast")]
        public void RootFolderGetsAddedToStorage() {
            string path = Path.GetTempPath();
            RepoInfo repoInfo = this.CreateRepoInfo(path);
            var activityListener = new Mock<IActivityListener>().Object;
            
            var session = new Mock<ISession>();
            var remoteObject = new Mock<IFolder>();
            remoteObject.Setup(r => r.Id).Returns("id");

            session.Setup(s => s.GetObjectByPath(It.IsAny<string>())).Returns(remoteObject.Object);

            var repo = new CmisRepoWrapper(repoInfo, activityListener, session.Object, new SingleStepEventQueue(new SyncEventManager()));
            repo.Queue.AddEvent(new SuccessfulLoginEvent(new Uri("http://example.com"), session.Object));
            var fsInfo = new DirectoryInfoWrapper(new DirectoryInfo(path));
            repo.singleStepQueue.Run();

            Assert.That(repo.DB.GetObjectByRemoteId("id"), Is.Not.Null);
            Assert.That(repo.DB.GetObjectByLocalPath(fsInfo), Is.Not.Null);
        }

        private RepoInfo CreateRepoInfo(string path) {
            return new RepoInfo
            {
                DisplayName = "name",
                Address = new Uri("http://example.com"),
                LocalPath = path,
                RemotePath = "/"
            };
        }

        private class CmisRepoWrapper : CmisRepo {
            public CmisRepoWrapper(RepoInfo repoInfo, IActivityListener activityListener, ISession session, SingleStepEventQueue queue) :
                base(repoInfo, activityListener, true, queue)
            {
                this.session = session;
                this.singleStepQueue = queue;
                this.Queue = this.singleStepQueue;
            }

            public SingleStepEventQueue singleStepQueue;

            public IMetaDataStorage DB {
                get { return this.storage; }
            }
        }
    }
}
