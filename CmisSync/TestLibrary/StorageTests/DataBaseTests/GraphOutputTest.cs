//-----------------------------------------------------------------------
// <copyright file="GraphOutputTest.cs" company="GRAU DATA AG">
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
        public void WritingStringTreeToStream() {
            var underTest = this.CreateStringTree();
            using (var stream = new MemoryStream()) {
                underTest.ToDotStream(stream);
                var content = System.Text.Encoding.Default.GetString(stream.ToArray());
                Console.WriteLine(content);
            }
        }

        [Test, Category("Fast")]
        public void WritingFileTreeToStream() {
            var underTest = this.CreateFileTree();
            using (var stream = new MemoryStream()) {
                underTest.ToDotStream(stream);
                var content = System.Text.Encoding.Default.GetString(stream.ToArray());
                Console.WriteLine(content);
            }
        }

        [Test, Category("Fast")]
        public void WritingTreeToFileInfo() {
            var underTest = this.CreateStringTree();
            var file = new Mock<IFileInfo>();
            file.Setup(f => f.Open(System.IO.FileMode.CreateNew)).Returns(() => new MemoryStream());

            underTest.ToDotFile(file.Object);

            file.Verify(f => f.Open(FileMode.CreateNew), Times.Once());
        }

        private IObjectTree<string> CreateStringTree() {
            var tree = new ObjectTree<string>() { Item = "root" };
            var children = new List<IObjectTree<string>>();
            children.Add(new ObjectTree<string>() { Item = "A" });
            children.Add(new ObjectTree<string>() { Item = "B" });
            tree.Children = children;
            return tree;
        }

        private IObjectTree<IFileSystemInfo> CreateFileTree() {
            var tree = new ObjectTree<IFileSystemInfo>() { Item = Mock.Of<IFileSystemInfo>(m => m.Name == "root") };
            var children = new List<IObjectTree<IFileSystemInfo>>();
            children.Add(new ObjectTree<IFileSystemInfo>() { Item = Mock.Of<IFileSystemInfo>(m => m.Name == "A") });
            children.Add(new ObjectTree<IFileSystemInfo>() { Item = Mock.Of<IFileSystemInfo>(m => m.Name == "B") });
            tree.Children = children;
            return tree;
        }
    }
}