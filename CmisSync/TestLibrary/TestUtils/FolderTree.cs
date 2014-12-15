//-----------------------------------------------------------------------
// <copyright file="FolderTree.cs" company="GRAU DATA AG">
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

            this.Children.Sort((FolderTree x, FolderTree y) => x.Name.CompareTo(y.Name));
        }

        public FolderTree(IFolder folder, string name = null) {
            this.Name = name ?? folder.Name;
            foreach (var child in folder.GetChildren()) {
                if (child is IFolder) {
                    this.Children.Add(new FolderTree(child as IFolder));
                }
            }

            this.Children.Sort((FolderTree x, FolderTree y) => x.Name.CompareTo(y.Name));
        }

        public FolderTree(IDirectoryInfo dir, string name = null) {
            this.Name = name ?? dir.Name;
            foreach (var child in dir.GetDirectories()) {
                this.Children.Add(new FolderTree(child));
            }

            this.Children.Sort((FolderTree x, FolderTree y) => x.Name.CompareTo(y.Name));
        }

        public FolderTree(DirectoryInfo dir, string name = null) {
            this.Name = name ?? dir.Name;
            foreach (var child in dir.GetDirectories()) {
                this.Children.Add(new FolderTree(child));
            }

            this.Children.Sort((FolderTree x, FolderTree y) => x.Name.CompareTo(y.Name));
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

        public override bool Equals(object obj)
        {
            return this.ToString().Equals(obj.ToString());
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }
}