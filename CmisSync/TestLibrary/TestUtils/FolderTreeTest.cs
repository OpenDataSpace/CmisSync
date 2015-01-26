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

namespace TestLibrary.TestUtils
{
    using System;

    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class FolderTreeTest
    {
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
            string treeReorderd =@".
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
            var A = Mock.Of<IDirectoryInfo>(d => d.Name == "A");
            var B = Mock.Of<IDirectoryInfo>(d => d.Name == "B");
            var C = Mock.Of<IDirectoryInfo>(d => d.Name == "C");
            var D = Mock.Of<IDirectoryInfo>(d => d.Name == "D");
            var E = Mock.Of<IDirectoryInfo>(d => d.Name == "E");
            var F = Mock.Of<IDirectoryInfo>(d => d.Name == "F");
            var G = Mock.Of<IDirectoryInfo>(d => d.Name == "G");
            Mock.Get(C).SetupDirectories(D);
            Mock.Get(E).SetupDirectories(F, G);
            Mock.Get(A).SetupDirectories(E);
            root.SetupDirectories(A, C, B);

            var underTest = new FolderTree(root.Object, ".");

            Assert.That(underTest, Is.EqualTo(new FolderTree(this.tree)));
        }
        
        [Test, Category("Fast")]
        public void ConstructFolderTreeByRemoteFolder() {
            var root = new Mock<IFolder>();
            var A = Mock.Of<IFolder>(d => d.Name == "A");
            var B = Mock.Of<IFolder>(d => d.Name == "B");
            var C = Mock.Of<IFolder>(d => d.Name == "C");
            var D = Mock.Of<IFolder>(d => d.Name == "D");
            var E = Mock.Of<IFolder>(d => d.Name == "E");
            var F = Mock.Of<IFolder>(d => d.Name == "F");
            var G = Mock.Of<IFolder>(d => d.Name == "G");
            Mock.Get(A).SetupChildren(E);
            Mock.Get(B).SetupChildren();
            Mock.Get(C).SetupChildren(D);
            Mock.Get(D).SetupChildren();
            Mock.Get(E).SetupChildren(F, G);
            Mock.Get(F).SetupChildren();
            Mock.Get(G).SetupChildren();
            root.SetupChildren(A, C, B);

            var underTest = new FolderTree(root.Object, ".");

            Assert.That(underTest, Is.EqualTo(new FolderTree(this.tree)));
        }

        [Test, Category("Fast")]
        public void ConstructFolderTreeByRemoteFolderWithFiles() {
            var root = new Mock<IFolder>();
            var A = Mock.Of<IFolder>(d => d.Name == "A");
            var B = Mock.Of<IFolder>(d => d.Name == "B");
            var C = Mock.Of<IFolder>(d => d.Name == "C");
            var D = Mock.Of<IDocument>(d => d.Name == "D");
            var E = Mock.Of<IFolder>(d => d.Name == "E");
            var F = Mock.Of<IDocument>(d => d.Name == "F");
            var G = Mock.Of<IFolder>(d => d.Name == "G");
            Mock.Get(A).SetupChildren(E);
            Mock.Get(B).SetupChildren();
            Mock.Get(C).SetupChildren(D);
            Mock.Get(E).SetupChildren(F, G);
            Mock.Get(G).SetupChildren();
            root.SetupChildren(A, C, B);

            var underTest = new FolderTree(root.Object, ".");

            Assert.That(underTest, Is.EqualTo(new FolderTree(this.tree)));
        }

        [Test, Category("Fast")]
        public void ConstructDiffenrentFolderTreeByLocalDirectory() {
            var root = new Mock<IDirectoryInfo>();
            var A = Mock.Of<IDirectoryInfo>(d => d.Name == "A");
            var B = Mock.Of<IDirectoryInfo>(d => d.Name == "B");
            var C = Mock.Of<IDirectoryInfo>(d => d.Name == "C");
            var D = Mock.Of<IDirectoryInfo>(d => d.Name == "D");
            var E = Mock.Of<IDirectoryInfo>(d => d.Name == "E");
            var F = Mock.Of<IDirectoryInfo>(d => d.Name == "F");
            var G = Mock.Of<IDirectoryInfo>(d => d.Name == "G");
            Mock.Get(C).SetupDirectories(E);
            Mock.Get(E).SetupDirectories(F, G);
            Mock.Get(A).SetupDirectories(D);
            root.SetupDirectories(A, C, B);

            var underTest = new FolderTree(root.Object, ".");

            Assert.That(underTest, Is.Not.EqualTo(new FolderTree(this.tree)));
        }

        [Test, Category("Fast")]
        public void ConstructDiffenrentFolderTreeByLocalDirectoryWithFiles() {
            var root = new Mock<IDirectoryInfo>();
            var A = Mock.Of<IDirectoryInfo>(d => d.Name == "A");
            var B = Mock.Of<IDirectoryInfo>(d => d.Name == "B");
            var C = Mock.Of<IDirectoryInfo>(d => d.Name == "C");
            var D = Mock.Of<IFileInfo>(d => d.Name == "D");
            var E = Mock.Of<IDirectoryInfo>(d => d.Name == "E");
            var F = Mock.Of<IFileInfo>(d => d.Name == "F");
            var G = Mock.Of<IDirectoryInfo>(d => d.Name == "G");
            Mock.Get(C).SetupDirectories(E);
            Mock.Get(E).SetupDirectories(G);
            Mock.Get(E).SetupFiles(F);
            Mock.Get(A).SetupFiles(D);
            root.SetupDirectories(A, C, B);

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
            string differentTree =@".
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
        public void IgnoreBlankLines() {
            Assert.That(new FolderTree(this.tree), Is.EqualTo(new FolderTree(this.tree2)));
        }
    }
}