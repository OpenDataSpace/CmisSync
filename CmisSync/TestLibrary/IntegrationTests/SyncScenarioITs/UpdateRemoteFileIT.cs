//-----------------------------------------------------------------------
// <copyright file="UpdateRemoteFileIT.cs" company="GRAU DATA AG">
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
namespace TestLibrary.IntegrationTests.SyncScenarioITs {
    using System;
    using System.IO;
    using System.Linq;

    using CmisSync.Lib;
    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture, Category("Slow"), TestName("UpdateRemoteFile"), Timeout(180000)]
    public class UpdateRemoteFileIT : AbstractBaseSyncScenarioIT {
        [Test]
        public void OneRemoteFileContentIsDeleted([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            var doc = this.remoteRootDir.CreateDocument(defaultFileName, defaultContent);

            InitializeAndRunRepo();

            doc.Refresh();
            doc.AssertThatIfContentHashExistsItIsEqualTo(defaultContent);
            byte[] hash = doc.ContentStreamHash();
            string oldChangeToken = doc.ChangeToken;
            doc.DeleteContentStream(true);
            string newChangeToken = doc.ChangeToken;
            Assert.That(oldChangeToken, Is.Not.EqualTo(newChangeToken));
            Assert.That(doc.ContentStreamLength, Is.Null.Or.EqualTo(0));
            doc.AssertThatIfContentHashExistsItIsEqualTo(string.Empty, string.Format("old hash was {0}", hash != null ? Utils.ToHexString(hash) : "null"));
            WaitForRemoteChanges(sleepDuration: 15000);
            AddStartNextSyncEvent();
            repo.Run();

            var children = this.localRootDir.GetFileSystemInfos();
            Assert.That(children.Length, Is.EqualTo(1));
            var file = children.First();
            Assert.That(file, Is.InstanceOf(typeof(FileInfo)));
            Assert.That((file as FileInfo).Length, Is.EqualTo(0));
            AssertThatFolderStructureIsEqual();
            AssertThatEventCounterIsZero();
        }

        [Test]
        public void OneRemoteFileUpdated([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            var doc = this.remoteRootDir.CreateDocument(defaultFileName, defaultContent);
            var newContent = defaultContent + defaultContent;

            InitializeAndRunRepo();

            doc.Refresh();
            doc.SetContent(newContent);

            WaitForRemoteChanges();
            AddStartNextSyncEvent();
            repo.Run();

            var file = this.localRootDir.GetFileSystemInfos().First();
            Assert.That(file, Is.InstanceOf(typeof(FileInfo)));
            Assert.That((file as FileInfo).Length, Is.EqualTo(newContent.Length));
            AssertThatFolderStructureIsEqual();
            AssertThatEventCounterIsZero();
        }
    }
}