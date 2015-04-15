//-----------------------------------------------------------------------
// <copyright file="FolderTreeTest.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class FolderTreeTest {
        private readonly string tree = @".
├── A
│   └── E
│       ├── F
│       └── G
├── B
└── C
    └── D
";

        private readonly string tree2 = @"
.
├── A
│   └── E
│       ├── F
│       └── G
├── B
└── C
    └── D";

        [Test, Category("Fast")]
        public void ConstructFolderTreeByString()
        {
            var underTest = new FolderTree(this.tree);

            Assert.That(underTest.ToString(), Is.EqualTo(this.tree));
        }

        [Test, Category("Fast")]
        public void DifferentOrderingsWillProduceEqualOutput() {
            string treeReorderd = @".
├── B
├── A
│   └── E
│       ├── G
│       └── F
└── C
    └── D
";
            var underTest = new FolderTree(treeReorderd);

            Assert.That(underTest, Is.EqualTo(new FolderTree(this.tree)));
        }

        [Test, Category("Fast")]
        public void ConstructFolderTreeByLocalDirectory() {
            var root = new Mock<IDirectoryInfo>();
            var a = Mock.Of<IDirectoryInfo>(dir => dir.Name == "A");
            var b = Mock.Of<IDirectoryInfo>(dir => dir.Name == "B");
            var c = Mock.Of<IDirectoryInfo>(dir => dir.Name == "C");
            var d = Mock.Of<IDirectoryInfo>(dir => dir.Name == "D");
            var e = Mock.Of<IDirectoryInfo>(dir => dir.Name == "E");
            var f = Mock.Of<IDirectoryInfo>(dir => dir.Name == "F");
            var g = Mock.Of<IDirectoryInfo>(dir => dir.Name == "G");
            Mock.Get(c).SetupDirectories(d);
            Mock.Get(e).SetupDirectories(f, g);
            Mock.Get(a).SetupDirectories(e);
            root.SetupDirectories(a, c, b);

            var underTest = new FolderTree(root.Object, ".");

            Assert.That(underTest, Is.EqualTo(new FolderTree(this.tree)));
        }
        
        [Test, Category("Fast")]
        public void ConstructFolderTreeByRemoteFolder() {
            var root = new Mock<IFolder>();
            var remoteRootId = Guid.NewGuid().ToString();
            root.Setup(r => r.Id).Returns(remoteRootId);
            var a = Mock.Of<IFolder>(dir => dir.Name == "A" && dir.Id == "1");
            var b = Mock.Of<IFolder>(dir => dir.Name == "B" && dir.Id == "2");
            var c = Mock.Of<IFolder>(dir => dir.Name == "C" && dir.Id == "3");
            var d = Mock.Of<IFolder>(dir => dir.Name == "D" && dir.Id == "4");
            var e = Mock.Of<IFolder>(dir => dir.Name == "E" && dir.Id == "5");
            var f = Mock.Of<IFolder>(dir => dir.Name == "F" && dir.Id == "6");
            var g = Mock.Of<IFolder>(dir => dir.Name == "G" && dir.Id == "7");
            Mock.Get(a).SetupChildren(e);
            Mock.Get(b).SetupChildren();
            Mock.Get(c).SetupChildren(d);
            Mock.Get(d).SetupChildren();
            Mock.Get(e).SetupChildren(f, g);
            Mock.Get(f).SetupChildren();
            Mock.Get(g).SetupChildren();
            root.SetupChildren(a, c, b);

            var underTest = new FolderTree(root.Object, ".");

            Assert.That(underTest, Is.EqualTo(new FolderTree(this.tree)));
        }

        [Test, Category("Fast")]
        public void ConstructFolderTreeByRemoteFolderWithFiles() {
            var root = new Mock<IFolder>();
            var a = Mock.Of<IFolder>(dir => dir.Name == "A");
            var b = Mock.Of<IFolder>(dir => dir.Name == "B");
            var c = Mock.Of<IFolder>(dir => dir.Name == "C");
            var d = Mock.Of<IDocument>(doc => doc.Name == "D");
            var e = Mock.Of<IFolder>(dir => dir.Name == "E");
            var f = Mock.Of<IDocument>(doc => doc.Name == "F");
            var g = Mock.Of<IFolder>(dir => dir.Name == "G");
            Mock.Get(a).SetupChildren(e);
            Mock.Get(b).SetupChildren();
            Mock.Get(c).SetupChildren(d);
            Mock.Get(e).SetupChildren(f, g);
            Mock.Get(g).SetupChildren();
            root.SetupChildren(a, c, b);

            var underTest = new FolderTree(root.Object, ".");

            Assert.That(underTest, Is.EqualTo(new FolderTree(this.tree)));
        }

        [Test, Category("Fast")]
        public void ConstructDiffenrentFolderTreeByLocalDirectory() {
            var root = new Mock<IDirectoryInfo>();
            var a = Mock.Of<IDirectoryInfo>(dir => dir.Name == "A");
            var b = Mock.Of<IDirectoryInfo>(dir => dir.Name == "B");
            var c = Mock.Of<IDirectoryInfo>(dir => dir.Name == "C");
            var d = Mock.Of<IDirectoryInfo>(dir => dir.Name == "D");
            var e = Mock.Of<IDirectoryInfo>(dir => dir.Name == "E");
            var f = Mock.Of<IDirectoryInfo>(dir => dir.Name == "F");
            var g = Mock.Of<IDirectoryInfo>(dir => dir.Name == "G");
            Mock.Get(c).SetupDirectories(e);
            Mock.Get(e).SetupDirectories(f, g);
            Mock.Get(a).SetupDirectories(d);
            root.SetupDirectories(a, c, b);

            var underTest = new FolderTree(root.Object, ".");

            Assert.That(underTest, Is.Not.EqualTo(new FolderTree(this.tree)));
        }

        [Test, Category("Fast")]
        public void ConstructDiffenrentFolderTreeByLocalDirectoryWithFiles() {
            var root = new Mock<IDirectoryInfo>();
            var a = Mock.Of<IDirectoryInfo>(dir => dir.Name == "A");
            var b = Mock.Of<IDirectoryInfo>(dir => dir.Name == "B");
            var c = Mock.Of<IDirectoryInfo>(dir => dir.Name == "C");
            var d = Mock.Of<IFileInfo>(doc => doc.Name == "D");
            var e = Mock.Of<IDirectoryInfo>(dir => dir.Name == "E");
            var f = Mock.Of<IFileInfo>(doc => doc.Name == "F");
            var g = Mock.Of<IDirectoryInfo>(dir => dir.Name == "G");
            Mock.Get(c).SetupDirectories(e);
            Mock.Get(e).SetupDirectories(g);
            Mock.Get(e).SetupFiles(f);
            Mock.Get(a).SetupFiles(d);
            root.SetupDirectories(a, c, b);

            var underTest = new FolderTree(root.Object, ".");

            Assert.That(underTest, Is.Not.EqualTo(new FolderTree(this.tree)));
        }

        [Test, Category("Fast")]
        public void EqualTrees() {
            string tree = @".
├── A
│   └── E
│       ├── F
│       └── G
├── B
└── C
    └── D
";
            Assert.That(new FolderTree(tree), Is.EqualTo(new FolderTree(tree)));
            var underTest = new FolderTree(tree).ToString();

            Assert.That(underTest, Is.EqualTo(new FolderTree(tree)));
        }

        [Test, Category("Fast")]
        public void NotEqualTrees() {
            string differentTree = @".
├── A
│   └── K
│       ├── F
│       └── G
├── B
└── C
    └── D
";
            var underTest = new FolderTree(this.tree);

            Assert.That(underTest, Is.Not.EqualTo(new FolderTree(differentTree)));
        }

        [Test, Category("Fast")]
        public void 

        [Test, Category("Fast")]
        public void AddLocalAndRemoteIdToFileName() {
            string localId = Guid.NewGuid().ToString();
            string remoteId = Guid.NewGuid().ToString();
            string name = "A";
            string nameWithIds = name + " {\"lid\": \"" + localId + "\", \"rid\": \"" + remoteId + "\"}";
            var underTest = new FolderTree(nameWithIds);

            Assert.That(underTest.Name, Is.EqualTo(name));
            Assert.That(underTest.LocalId, Is.EqualTo(localId));
            Assert.That(underTest.RemoteId, Is.EqualTo(remoteId));
            Assert.That(underTest, Is.EqualTo(new FolderTree(underTest.ToString())));
            Assert.That(underTest.ToString(), Is.StringContaining(localId).And.StringContaining(remoteId));
        }

        [Test, Category("Fast")]
        public void ConvertStringToTree() {
            FolderTree underTest = this.tree;
            Assert.That(underTest, Is.EqualTo(new FolderTree(this.tree)));
        }

        [Test, Category("Fast")]
        public void ConvertTreeToString() {
            string newTree = new FolderTree(this.tree);
            Assert.That(newTree, Is.EqualTo(this.tree));
        }

        [Test, Category("Fast")]
        public void IgnoreBlankLines() {
            Assert.That(new FolderTree(this.tree), Is.EqualTo(new FolderTree(this.tree2)));
        }
    }
}