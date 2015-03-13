//-----------------------------------------------------------------------
// <copyright file="MockOfIFolderUtil.cs" company="GRAU DATA AG">
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

namespace TestLibrary.TestUtils {
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS;
    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    public static class MockOfIFolderUtil {
        /// <summary>
        /// Setups the descendants.
        /// </summary>
        /// <param name="folder">Folder to be setup.</param>
        /// <param name="children">Children of the folder. The children could contain children as well and they will be used to create the full tree.</param>
        public static void SetupDescendants(this Mock<IFolder> folder, params IFileableCmisObject[] children) {
            var list = new List<ITree<IFileableCmisObject>>();
            foreach (var child in children) {
                var tree = new Mock<ITree<IFileableCmisObject>>();
                tree.Setup(t => t.Item).Returns(child);
                tree.Setup(t => t.Children).Returns(child is IFolder ? (child as IFolder).GetDescendants(-1) : new List<ITree<IFileableCmisObject>>());
                list.Add(tree.Object);
            }

            folder.Setup(f => f.GetDescendants(-1)).Returns(list);
            folder.Setup(f => f.GetDescendants(It.Is<int>(d => d != -1))).Throws(new ArgumentOutOfRangeException("Get Descendants should not be limited"));
        }

        /// <summary>
        /// Setup the children of a folder.
        /// </summary>
        /// <param name="folder">Folder containing the children.</param>
        /// <param name="children">The children of the folder.</param>
        public static void SetupChildren(this Mock<IFolder> folder, params ICmisObject[] children) {
            var list = new Mock<IItemEnumerable<ICmisObject>>();
            var internalList = new List<ICmisObject>(children);
            list.Setup(l => l.TotalNumItems).Returns(children.LongLength);
            list.Setup(l => l.GetEnumerator()).Returns(internalList.GetEnumerator());
            folder.Setup(f => f.GetChildren()).Returns(list.Object);
        }

        public static void SetupLastModificationDate(this Mock<IFolder> folder, DateTime? modificationDate) {
            folder.Setup(f => f.LastModificationDate).Returns(modificationDate);
            folder.Setup(f => f.UpdateProperties(It.IsAny<IDictionary<string, object>>(), true)).Callback<IDictionary<string, object>, bool>((d, b) => {
                if (d.ContainsKey(PropertyIds.LastModificationDate)) {
                    folder.SetupLastModificationDate((DateTime?)d[PropertyIds.LastModificationDate]);
                }
            });
        }

        public static void SetupChangeToken(this Mock<IFolder> folder, string changeToken) {
            folder.Setup(f => f.ChangeToken).Returns(changeToken);
        }

        public static void SetupIgnore(this Mock<IFolder> folder, params string[] devices) {
            IList<IProperty> props = new List<IProperty>();
            if (folder.Object.Properties != null) {
                foreach (var prop in folder.Object.Properties) {
                    if (prop.Id != "gds:ignoreDeviceIds") {
                        props.Add(prop);
                    }
                }
            }

            props.Add(Mock.Of<IProperty>(p => p.Id == "gds:ignoreDeviceIds" && p.Values == new List<object>(devices)));

            folder.Setup(f => f.Properties).Returns(props);
        }

        public static Mock<IFolder> CreateRemoteFolderMock(string id, string name, string path, string parentId = null, string changetoken = "changetoken", bool ignored = false) {
            var newRemoteObject = new Mock<IFolder>();
            newRemoteObject.Setup(d => d.Id).Returns(id);
            newRemoteObject.Setup(d => d.Path).Returns(path);
            newRemoteObject.Setup(d => d.ParentId).Returns(parentId);
            newRemoteObject.Setup(d => d.Parents).Returns(new List<IFolder>() { Mock.Of<IFolder>(f => f.Id == parentId) });
            newRemoteObject.Setup(d => d.Name).Returns(name);
            newRemoteObject.Setup(d => d.ChangeToken).Returns(changetoken);
            newRemoteObject.Setup(d => d.GetDescendants(It.IsAny<int>())).Returns(new List<ITree<IFileableCmisObject>>());
            newRemoteObject.Setup(d => d.Move(It.IsAny<IObjectId>(), It.IsAny<IObjectId>())).Returns((IObjectId old, IObjectId current) => CreateRemoteFolderMock(id, name, path, current.Id, changetoken).Object);
            newRemoteObject.SetupIgnoreFlag(ignored);
            return newRemoteObject;
        }

        public static void SetupIgnoreFlag(this Mock<IFolder> folder, bool ignored) {
            var properties = folder.Object.Properties ?? new List<IProperty>();
            if (ignored && !folder.Object.AreAllChildrenIgnored()) {
                var ignoreEntry = new Mock<IProperty>();
                ignoreEntry.Setup(i => i.Id).Returns("gds:ignoreDeviceIds");
                IList<object> values = new List<object>();
                values.Add("*");
                ignoreEntry.Setup(i => i.Values).Returns(values);
                properties.Add(ignoreEntry.Object);
            }

            if (!ignored && folder.Object.AreAllChildrenIgnored()) {
                foreach (var prop in properties) {
                    if (prop.Id == "gds:ignoreDeviceIds") {
                        properties.Remove(prop);
                    }
                }
            }

            folder.Setup(d => d.Properties).Returns(properties);
        }

        public static void VerifyUpdateLastModificationDate(this Mock<IFolder> folder, DateTime modificationDate, bool refresh = true) {
            folder.VerifyUpdateLastModificationDate(modificationDate, Times.Once(), refresh);
        }

        public static void VerifyUpdateLastModificationDate(this Mock<IFolder> folder, DateTime modificationDate, Times times, bool refresh = true) {
            folder.Verify(d => d.UpdateProperties(It.Is<IDictionary<string, object>>(dic => VerifyDictContainsLastModification(dic, modificationDate)), refresh));
        }

        private static bool VerifyDictContainsLastModification(IDictionary<string, object> dic, DateTime modificationDate) {
            Assert.That(dic.ContainsKey(PropertyIds.LastModificationDate));
            Assert.That(dic[PropertyIds.LastModificationDate], Is.EqualTo(modificationDate));
            return true;
        }
    }
}