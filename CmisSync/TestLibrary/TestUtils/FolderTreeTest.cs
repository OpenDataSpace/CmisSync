
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
        private readonly string tree =
            ".\n" +
                "├── A\n" +
                "│   └── E\n" +
                "│       ├── F\n" +
                "│       └── G\n" +
                "├── B\n" +
                "└── C\n" +
                "    └── D\n";

        private readonly string tree2 = @"
.
├── A
│   └── E
│       ├── F
│       └── G
├── B
└── C
    └── D
";

        [Test, Category("Fast")]
        public void ConstructFolderTreeByString()
        {
            var underTest = new FolderTree(this.tree);

            Assert.That(underTest.ToString(), Is.EqualTo(this.tree));
        }

        [Test, Category("Fast")]
        public void DifferentOrderingsWillProduceEqualOutput() {
            string treeReorderd =@"
.
├── B
├── A
│   └── E
│       ├── G
│       └── F
└── C
    └── D";
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
        public void EqualTrees() {
            string tree =
                ".\n" +
                    "├── A\n" +
                    "│   └── E\n" +
                    "│       ├── F\n" +
                    "│       └── G\n" +
                    "├── B\n" +
                    "└── C\n" +
                    "    └── D\n";
            var underTest = new FolderTree(tree);

            Assert.That(underTest, Is.EqualTo(new FolderTree(tree)));
        }

        [Test, Category("Fast")]
        public void NotEqualTrees() {
            string differentTree =
                ".\n" +
                    "├── A\n" +
                    "│   └── K\n" +
                    "│       ├── F\n" +
                    "│       └── G\n" +
                    "├── B\n" +
                    "└── C\n" +
                    "    └── D\n";
            var underTest = new FolderTree(this.tree);

            Assert.That(underTest, Is.Not.EqualTo(new FolderTree(differentTree)));
        }

        [Test, Category("Fast")]
        public void IgnoreBlankLines() {
            Assert.That(new FolderTree(this.tree), Is.EqualTo(new FolderTree(this.tree2)));
        }
    }
}