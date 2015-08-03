//-----------------------------------------------------------------------
// <copyright file="DescendantsCrawlerLargeTests.cs" company="GRAU DATA AG">
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
namespace TestLibrary.ProducerTests.CrawlerTests {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    using CmisSync.Lib;
    using CmisSync.Lib.Consumer;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Exceptions;
    using CmisSync.Lib.Filter;
    using CmisSync.Lib.PathMatcher;
    using CmisSync.Lib.Producer.Crawler;
    using CmisSync.Lib.Producer.Watcher;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.SelectiveIgnore;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DBreeze;

    using DotCMIS.Client;
    using DotCMIS.Client.Impl;

    using Moq;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture, Category("Slow")]
    public class DescendantsCrawlerLargeTests : DescendantsCrawlerTest {
        [Test]
        public void MassiveTreeSizes(
            [Range(1, 3)]int subfolderPerFolder,
            [Range(1, 3)]int folderDepth,
            [Range(0, 100, 10)]int filesPerFolder)
        {
            this.remoteFolder.SetupId(remoteRootId);
            var remoteTree = this.CreateRemoteChildren(subfolderPerFolder, folderDepth, filesPerFolder, remoteRootId, "/");
            var localTree = this.CreateLocalChildren(subfolderPerFolder, folderDepth, filesPerFolder, Path.GetTempPath());
            this.SaveStoredTree(subfolderPerFolder, folderDepth, filesPerFolder, remoteRootId, "/");
            this.remoteFolder.Setup(f => f.GetDescendants(-1)).Returns(remoteTree);
            this.localFolder.SetupFilesAndDirectories(localTree);

            var underTest = this.CreateCrawler();

            var watch = new Stopwatch();
            watch.Start();
            Assert.That(underTest.Handle(new StartNextSyncEvent()), Is.True);
            watch.Stop();
            Console.WriteLine("Crawl took " + watch.ElapsedMilliseconds + " msec");
            this.queue.VerifyThatNoOtherEventIsAddedThan<FullSyncCompletedEvent>();
        }

        private IList<ITree<IFileableCmisObject>> CreateRemoteChildren(int subFolder, int folderDepth, int filesPerFolder, string parentId, string parentPath) {
            IList<ITree<IFileableCmisObject>> trees = new List<ITree<IFileableCmisObject>>();
            for (int i = 0; i < filesPerFolder; i++) {
                var id = string.Format("{0}/{1}.doc", parentPath, i);
                var doc = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, id, string.Format("{0}.doc", i), parentId, 0);
                doc.SetupPath(string.Format("{0}/{1}.doc", parentPath, i));
                var tree = new Tree<IFileableCmisObject> {
                    Item = doc.Object,
                    Children = null
                };
                trees.Add(tree);
            }

            if (folderDepth > 0) {
                for (int i = 0; i < subFolder; i++) {
                    var id = string.Format("{0}/{1}/", parentPath, i);
                    var folder = MockOfIFolderUtil.CreateRemoteFolderMock(id, i.ToString(), parentPath + "/" + i, parentId);
                    var tree = new Tree<IFileableCmisObject> {
                        Item = folder.Object,
                        Children = this.CreateRemoteChildren(subFolder, folderDepth - 1, filesPerFolder, id, folder.Object.Path)
                    };
                    trees.Add(tree);
                }
            }

            return trees;
        }

        private IFileSystemInfo[] CreateLocalChildren(int subFolder, int folderDepth, int filesPerFolder, string parentPath) {
            var children = new List<IFileSystemInfo>();
            for (int i = 0; i < filesPerFolder; i++) {
                var name = string.Format("{0}.doc", i);
                var path = Path.Combine(parentPath, name);
                var doc = new Mock<IFileInfo>(MockBehavior.Strict).SetupFullName(path).SetupName(name).SetupExists().SetupSymlink();
                var guid = Guid.NewGuid();
                doc.SetupGuid(guid).SetupLastWriteTimeUtc(new DateTime(2000, 1, 1)).SetupReadOnly(false);
                this.localGuids.Enqueue(guid);
                children.Add(doc.Object);
            }

            if (folderDepth > 0) {
                for (int i = 0; i < subFolder; i++) {
                    var name = string.Format("{0}", i);
                    var path = Path.Combine(parentPath, name);
                    var folder = new Mock<IDirectoryInfo>(MockBehavior.Strict).SetupFullName(path).SetupName(name).SetupExists().SetupSymlink();
                    var guid = Guid.NewGuid();
                    folder.SetupGuid(guid).SetupLastWriteTimeUtc(new DateTime(2000, 1, 1)).SetupReadOnly(false);
                    this.localGuids.Enqueue(guid);
                    folder.SetupFilesAndDirectories(this.CreateLocalChildren(subFolder, folderDepth - 1, filesPerFolder, folder.Object.FullName));
                    children.Add(folder.Object);
                }
            }

            return children.ToArray();
        }

        private void SaveStoredTree(int subFolder, int folderDepth, int filesPerFolder, string parentId, string parentPath) {
            for (int i = 0; i < filesPerFolder; i++) {
                var doc = new MappedObject(
                    string.Format("{0}.doc", i),
                    string.Format("{0}/{1}.doc", parentPath, i),
                    MappedObjectType.File,
                    parentId,
                    "changetoken",
                    0)
                {
                    Guid = this.localGuids.Dequeue(),
                    LastLocalWriteTimeUtc = new DateTime(2000, 1, 1)
                };
                this.storage.SaveMappedObject(doc);
            }

            if (folderDepth > 0) {
                for (int i = 0; i < subFolder; i++) {
                    var remoteId = string.Format("{0}/{1}/", parentPath, i);
                    var folder = new MappedObject(
                        string.Format("{0}", i),
                        remoteId,
                        MappedObjectType.Folder,
                        parentId,
                        "changetoken")
                    {
                        Guid = this.localGuids.Dequeue(),
                        LastLocalWriteTimeUtc = new DateTime(2000, 1, 1)
                    };
                    this.storage.SaveMappedObject(folder);
                    this.SaveStoredTree(subFolder, folderDepth - 1, filesPerFolder, remoteId, parentPath + "/" + i);
                }
            }
        }
    }
}