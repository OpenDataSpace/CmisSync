//-----------------------------------------------------------------------
// <copyright file="MockSessionUtil.cs" company="GRAU DATA AG">
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

namespace TestLibrary.TestUtils
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage.FileSystem;
    using CmisSync.Lib.Producer.Watcher;
    using CmisSync.Lib.SelectiveIgnore;

    using DotCMIS.Binding;
    using DotCMIS.Binding.Services;
    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Data.Extensions;
    using DotCMIS.Enums;

    using Moq;

    using NUnit.Framework;

    public static class MockSessionUtil {
        public static void SetupSessionDefaultValues(this Mock<ISession> session) {
            session.Setup(s => s.Binding.GetRepositoryService().GetRepositoryInfos(null)).Returns((IList<IRepositoryInfo>)null);
            session.Setup(s => s.RepositoryInfo.Id).Returns("repoId");
        }

        public static void SetupChangeLogToken(this Mock<ISession> session, string changeLogToken) {
            session.Setup(s => s.Binding.GetRepositoryService().GetRepositoryInfo(It.IsAny<string>(), null).LatestChangeLogToken).Returns(changeLogToken);
        }

        public static void SetupCreateOperationContext(this Mock<ISession> session) {
            session.Setup(s => s.CreateOperationContext(
                It.IsAny<HashSet<string>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IncludeRelationshipsFlag>(),
                It.IsAny<HashSet<string>>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<int>())).Returns(
                (HashSet<string> filter,
             bool includeAcls,
             bool includeAllowableActions,
             bool includePolicies,
             IncludeRelationshipsFlag relationshipFlags,
             HashSet<string> renditions,
             bool includePathSegments,
             string orderBy,
             bool cacheEnabled,
             int maxItemsPerPage) => Mock.Of<IOperationContext>(
                o =>
                o.Filter == new HashSet<string>(filter) &&
                o.IncludeAcls == includeAcls &&
                o.IncludeAllowableActions == includeAllowableActions &&
                o.IncludePolicies == includePolicies &&
                o.IncludeRelationships == (IncludeRelationshipsFlag?)relationshipFlags &&
                o.RenditionFilter == new HashSet<string>(renditions) &&
                o.IncludePathSegments == includePathSegments &&
                o.OrderBy == orderBy &&
                o.CacheEnabled == cacheEnabled &&
                o.MaxItemsPerPage == maxItemsPerPage));
        }

        public static void SetupTypeSystem(this Mock<ISession> session, bool serverCanModifyLastModificationDate = true, bool supportsSelectiveIgnore = true) {
            string repoId = "repoId";
            IList<IPropertyDefinition> props = new List<IPropertyDefinition>();
            if (serverCanModifyLastModificationDate) {
                props.Add(Mock.Of<IPropertyDefinition>(p => p.Id == "cmis:lastModificationDate" && p.Updatability == DotCMIS.Enums.Updatability.ReadWrite));
            } else {
                props.Add(Mock.Of<IPropertyDefinition>(p => p.Id == "cmis:lastModificationDate" && p.Updatability == DotCMIS.Enums.Updatability.ReadOnly));
            }

            var docType = Mock.Of<IObjectType>(d => d.PropertyDefinitions == props);
            var folderType = Mock.Of<IObjectType>(d => d.PropertyDefinitions == props);

            Mock<IRepositoryService> repositoryService = new Mock<IRepositoryService>();

            if (session.Object.Binding != null && session.Object.Binding.GetRepositoryService() != null) {
                repositoryService = Mock.Get(session.Object.Binding.GetRepositoryService());
            }

            repositoryService.Setup(s => s.GetTypeDefinition(repoId, "cmis:document", null)).Returns(docType);
            repositoryService.Setup(s => s.GetTypeDefinition(repoId, "cmis:folder", null)).Returns(folderType);
            if (supportsSelectiveIgnore) {
                IList<IPropertyDefinition> syncProps = new List<IPropertyDefinition>();
                syncProps.Add(Mock.Of<IPropertyDefinition>(p => p.Id == "gds:ignoreDeviceIds"));
                var syncSecondaryObjects = Mock.Of<IObjectType>(d => d.PropertyDefinitions == syncProps);
                repositoryService.Setup(s => s.GetTypeDefinition(repoId, "gds:sync", null)).Returns(syncSecondaryObjects);
                session.Setup(s => s.GetTypeDefinition("gds:sync")).Returns(syncSecondaryObjects);
            }

            session.Setup(s => s.Binding.GetRepositoryService()).Returns(repositoryService.Object);
            session.Setup(s => s.RepositoryInfo.Id).Returns(repoId);
        }

        public static Mock<IChangeEvent> GenerateChangeEvent(DotCMIS.Enums.ChangeType type, string objectId) {
            var changeEvent = new Mock<IChangeEvent>();
            changeEvent.Setup(ce => ce.ObjectId).Returns(objectId);
            changeEvent.Setup(ce => ce.ChangeType).Returns(type);

            return changeEvent;
        }

        public static Mock<ISession> PrepareSessionMockForSingleChange(DotCMIS.Enums.ChangeType type, string objectId = "objectId", string changeLogToken = "token", string latestChangeLogToken = "latestChangeLogToken") {
            var changeEvents = new Mock<IChangeEvents>();
            var changeList = GenerateSingleChangeListMock(type, objectId); 
            changeEvents.Setup(ce => ce.HasMoreItems).Returns((bool?)false);
            changeEvents.Setup(ce => ce.LatestChangeLogToken).Returns(latestChangeLogToken);
            changeEvents.Setup(ce => ce.TotalNumItems).Returns(1);
            changeEvents.Setup(ce => ce.ChangeEventList).Returns(changeList);

            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            session.SetupChangeLogToken(changeLogToken);
            session.Setup(s => s.GetContentChanges(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<long>())).Returns(changeEvents.Object);
            return session;
        }

        public static void AddRemoteObject(this Mock<ISession> session, ICmisObject remoteObject) {
            session.Setup(s => s.GetObject(It.Is<string>(id => id == remoteObject.Id))).Returns(remoteObject);
            HashSet<string> paths = new HashSet<string>();
            if (remoteObject is IFolder)
            {
                paths.Add((remoteObject as IFolder).Path);
                if ((remoteObject as IFolder).Paths != null)
                {
                    foreach (string path in (remoteObject as IFolder).Paths)
                    {
                        paths.Add(path);
                    }
                }
            }
            else if (remoteObject is IDocument)
            {
                foreach (string path in (remoteObject as IDocument).Paths)
                {
                    paths.Add(path);
                }
            }

            foreach (string path in paths)
            {
                session.Setup(s => s.GetObjectByPath(It.Is<string>(p => p == path))).Returns(remoteObject);
            }
        }

        public static void AddRemoteObjects(this Mock<ISession> session, params ICmisObject[] remoteObjects) {
            foreach(var obj in remoteObjects) {
                session.AddRemoteObject(obj);
            }
        }

        public static Mock<IFolder> CreateCmisFolder(List<string> fileNames = null, List<string> folderNames = null, bool contentStream = false) {
            var remoteFolder = new Mock<IFolder>();
            var remoteChildren = new Mock<IItemEnumerable<ICmisObject>>();
            var list = new List<ICmisObject>();
            if (fileNames != null) {
                foreach (var name in fileNames) {
                    var doc = new Mock<IDocument>();
                    doc.Setup(d => d.Name).Returns(name);
                    if (contentStream) {
                        doc.Setup(d => d.ContentStreamId).Returns(name);
                    }

                    list.Add(doc.Object);
                }
            }

            if (folderNames != null) {
                foreach (var name in folderNames) {
                    var folder = new Mock<IFolder>();
                    folder.Setup(d => d.Name).Returns(name);
                    list.Add(folder.Object);
                }
            }

            remoteChildren.Setup(r => r.GetEnumerator()).Returns(list.GetEnumerator());
            remoteFolder.Setup(r => r.GetChildren()).Returns(remoteChildren.Object);
            return remoteFolder;
        }

        public static Mock<ISession> GetSessionMockReturningFolderChange(DotCMIS.Enums.ChangeType type, string id = "folderid", string folderName = "name", string path = "path", string parentId = "", string changetoken = "changetoken") {
            if (path.Contains("\\")) {
                throw new ArgumentException("Given remote path: " + path + " contains \\");
            }

            var session = PrepareSessionMockForSingleChange(type, id);
            var newRemoteObject = MockOfIFolderUtil.CreateRemoteFolderMock(id, folderName, path, parentId, changetoken);
            session.Setup(s => s.GetObject(It.IsAny<string>())).Returns(newRemoteObject.Object);
            session.Setup(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>())).Returns(newRemoteObject.Object);
         
            return session;
        }

        public static Mock<ISession> GetSessionMockReturning3Changesin2Batches(DotCMIS.Enums.ChangeType type = DotCMIS.Enums.ChangeType.Updated, bool overlapping = false) {
            var changeEvents = new Mock<IChangeEvents>();
            changeEvents.Setup(ce => ce.HasMoreItems).ReturnsInOrder((bool?)true, (bool?)false);
            changeEvents.Setup(ce => ce.LatestChangeLogToken).ReturnsInOrder("A", "B");
            changeEvents.Setup(ce => ce.TotalNumItems).ReturnsInOrder(3, overlapping ? 2 : 1);
            var event1 = MockSessionUtil.GenerateChangeEvent(type, "one");
            var event2 = MockSessionUtil.GenerateChangeEvent(type, "two");
            var event3 = MockSessionUtil.GenerateChangeEvent(type, "three");
            List<IChangeEvent> changeList1 = new List<IChangeEvent>();
            changeList1.Add(event1.Object);
            changeList1.Add(event2.Object);
            List<IChangeEvent> changeList2 = new List<IChangeEvent>();
            if (overlapping) {
                changeList2.Add(event2.Object);
            }

            changeList2.Add(event3.Object);
            changeEvents.Setup(ce => ce.ChangeEventList).ReturnsInOrder(changeList1, changeList2);

            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            session.Setup(s => s.Binding.GetRepositoryService().GetRepositoryInfo(It.IsAny<string>(), null).LatestChangeLogToken).Returns("token");
            session.Setup(s => s.GetContentChanges(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<long>())).Returns(changeEvents.Object);

            return session;
        }

        public static Mock<ISession> GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType type, string id, string documentContentStreamId = null) {
            var session = MockSessionUtil.PrepareSessionMockForSingleChange(type, id);

            var newRemoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(documentContentStreamId, id, "name", (string)null);
            session.Setup(s => s.GetObject(id, It.IsAny<IOperationContext>())).Returns(newRemoteObject.Object);
            session.Setup(s => s.GetObject(id)).Returns(newRemoteObject.Object);

            return session;
        }

        public static void VerifyThatCachingIsDisabled(this Mock<ISession> session)
        {
            session.Verify(
                s => s.CreateOperationContext(
                It.IsAny<HashSet<string>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IncludeRelationshipsFlag>(),
                It.IsAny<HashSet<string>>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.Is<bool>(b => b == false),
                It.IsAny<int>()),
                Times.Once());
        }

        public static void VerifyThatAllDefaultValuesAreSet(this Mock<ISession> session) {
            session.Verify(
                s => s.CreateOperationContext(
                It.Is<HashSet<string>>(set =>
                                   set.Contains("cmis:name") &&
                                   set.Contains("cmis:parentId") &&
                                   set.Contains("cmis:objectId") &&
                                   set.Contains("cmis:changeToken") &&
                                   set.Contains("cmis:contentStreamFileName") &&
                                   set.Contains("cmis:lastModificationDate")),
                It.Is<bool>(acls => acls == false),
                It.Is<bool>(includeAllowableActions => includeAllowableActions == true),
                It.Is<bool>(includePolicies => includePolicies == false),
                It.Is<IncludeRelationshipsFlag>(relationship => relationship == IncludeRelationshipsFlag.None),
                It.Is<HashSet<string>>(set => set.Contains("cmis:none") && set.Count == 1),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.Is<int>(i => i > 1)),
                Times.Once());
        }

        public static void VerifyThatCrawlValuesAreSet(this Mock<ISession> session) {
            session.Verify(
                s => s.CreateOperationContext(
                It.Is<HashSet<string>>(set =>
                                   !set.Contains("cmis:path")),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IncludeRelationshipsFlag>(),
                It.IsAny<HashSet<string>>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<int>()),
                Times.Once());
        }

        public static void VerifyThatFilterContainsPath(this Mock<ISession> session) {
            session.Verify(
                s => s.CreateOperationContext(
                It.Is<HashSet<string>>(set =>
                                   set.Contains("cmis:path")),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IncludeRelationshipsFlag>(),
                It.IsAny<HashSet<string>>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<int>()),
                Times.Once());
        }

        public static void EnsureSelectiveIgnoreSupportIsAvailable(this ISession session) {
            if (!session.SupportsSelectiveIgnore()) {
                Assert.Ignore("Selective Ignore is not available on server");
            }
        }

        private static List<IChangeEvent> GenerateSingleChangeListMock(DotCMIS.Enums.ChangeType type, string objectId = "objId") {
            var changeList = new List<IChangeEvent>();
            changeList.Add(GenerateChangeEvent(type, objectId).Object);
            return changeList;
        }
    }
}