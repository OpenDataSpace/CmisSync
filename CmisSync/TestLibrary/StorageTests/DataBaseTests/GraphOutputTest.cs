
namespace TestLibrary.StorageTests.DataBaseTests {
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class GraphOutputTest {
        [Test, Category("Fast")]
        public void WritingTreeToStream() {
            var underTest = this.CreateTree();
            using (var stream = new MemoryStream()) {
                underTest.ToDotStream(stream);
                var content = System.Text.Encoding.Default.GetString(stream.ToArray());
                Console.WriteLine(content);
            }
        }

        [Test, Category("Fast")]
        public void WritingTreeToFileInfo() {
            var underTest = this.CreateTree();
            var file = new Mock<IFileInfo>();
            file.Setup(f => f.Open(System.IO.FileMode.CreateNew)).Returns(() => new MemoryStream());

            underTest.ToDotFile(file.Object);

            file.Verify(f => f.Open(FileMode.CreateNew), Times.Once());
        }

        private IObjectTree<string> CreateTree() {
            var tree = new ObjectTree<string>() { Item = "root" };
            var children = new List<IObjectTree<string>>();
            children.Add(new ObjectTree<string>() { Item = "A" });
            children.Add(new ObjectTree<string>() { Item = "B" });
            tree.Children = children;
            return tree;
        }
    }
}