
namespace TestLibrary.TestUtils
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Text;

    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    public class FolderTree
    {
        private List<FolderTree> Children = new List<FolderTree>();
        public string Name { get; private set; }
        public FolderTree(string tree) {
            string[] lines = tree.Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            string child = string.Empty;
            var childTree = new StringBuilder();
            foreach (var line in lines) {
                if (line.StartsWith("├── ") || line.StartsWith("└── ")) {
                    if (child != string.Empty) {
                        this.Children.Add(new FolderTree(childTree.ToString()));
                    }

                    child = line.Substring(4);
                    childTree = new StringBuilder(child);
                } else if (line.StartsWith("│   ") || line.StartsWith("    ")) {
                    childTree.Append(System.Environment.NewLine).Append(line.Substring(4));
                } else {
                    this.Name = line;
                }
            }

            string lastChild = childTree.ToString();
            if (!string.IsNullOrWhiteSpace(lastChild)) {
                this.Children.Add(new FolderTree(lastChild));
            }
        }

        public override string ToString()
        {
            var tree = new StringBuilder();
            tree.AppendLine(this.Name);
            int count = this.Children.Count;
            foreach (var child in this.Children) {
                string prefix = "│   ";
                if (count <= 1 ) {
                    tree.Append("└── ");
                    prefix = "    ";
                } else {
                    tree.Append("├── ");
                }

                string[] subTreeLines = child.ToString().Split(new string[] { Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
                bool firstLine = true;
                foreach (var line in subTreeLines) {
                    if (firstLine) {
                        firstLine = false;
                    } else {
                        tree.Append(prefix);
                    }

                    tree.AppendLine(line);
                }

                count--;
            }

            return tree.ToString();
        }

        public void CreateTreeIn(IDirectoryInfo rootFolder, bool deleteNonListed = true) {

        }

        public void CreateTreeIn(IFolder rootFolder, bool deleteNonListed = true) {

        }
    }
}