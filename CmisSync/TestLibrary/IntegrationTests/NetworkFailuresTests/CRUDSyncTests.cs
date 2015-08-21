using CmisSync.Lib.Cmis;


namespace TestLibrary.IntegrationTests.NetworkFailuresTests {
    using System;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using NUnit.Framework;

    using TestUtils;
    using TestUtils.ToxiproxyUtils;

    using Toxiproxy.Net;

    [TestFixture, Timeout(60000), Category("Toxiproxy")]
    public class CRUDSyncTests : IsFullTestWithToxyProxy{
        [Test]
        public void RemoteFolderCreated(
            [Range(-1, 5)]int blockedRequest,
            [Values(1, 3)]int numberOfBlockedRequests,
            [Values(true, false)]bool contentChanges)
        {
            this.RetryOnInitialConnection = true;
            this.InitializeAndRunRepo(swallowExceptions: true);
            this.ContentChangesActive = contentChanges;

            string folderName = "testFolder";
            this.remoteRootDir.CreateFolder(folderName);
            int reqNumber = 0;
            this.AuthProviderWrapper.OnAuthenticate += (object obj) => {
                if (reqNumber >= blockedRequest && reqNumber < blockedRequest + numberOfBlockedRequests) {
                    this.Proxy.Disable();
                } else {
                    this.Proxy.Enable();
                }

                reqNumber ++;
                Assert.That(reqNumber, Is.LessThan(100));
            };

            this.WaitForRemoteChanges();
            for (int i = 0; i <= numberOfBlockedRequests; i++) {
                this.AddStartNextSyncEvent();
                this.repo.Run();
            }

            this.localRootDir.Refresh();
            this.remoteRootDir.Refresh();
            Assert.That(new FolderTree(this.localRootDir), Is.EqualTo(new FolderTree(this.remoteRootDir)));
        }
    }
}