//-----------------------------------------------------------------------
// <copyright file="CRUDSyncTests.cs" company="GRAU DATA AG">
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
﻿
namespace TestLibrary.IntegrationTests.NetworkFailuresTests {
    using System;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using NUnit.Framework;

    using TestUtils;
    using TestUtils.ToxiproxyUtils;

    using Toxiproxy.Net;

    [TestFixture, Category("Toxiproxy")]
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

        [Test]
        public void LocalFolderCreated(
            [Range(-1, 5)]int blockedRequest,
            [Values(1, 2, 3)]int numberOfBlockedRequests,
            [Values(true, false)]bool contentChanges)
        {
            this.RetryOnInitialConnection = true;
            this.InitializeAndRunRepo(swallowExceptions: true);
            this.ContentChangesActive = contentChanges;

            string folderName = "testFolder";
            this.localRootDir.CreateSubdirectory(folderName);
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

            this.WaitUntilQueueIsNotEmpty();
            this.repo.Run();

            for (int i = 0; i <= 3; i++) {
                this.AddStartNextSyncEvent();
                this.repo.Run();
            }

            this.localRootDir.Refresh();
            this.remoteRootDir.Refresh();
            Assert.That(new FolderTree(this.remoteRootDir), Is.EqualTo(new FolderTree(this.localRootDir)));
        }
    }
}