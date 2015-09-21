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

namespace TestLibrary.TestUtils {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Newtonsoft.Json.Linq;

    public class FolderTree {
        private List<FolderTree> children = new List<FolderTree>();

        public FolderTree(string tree) {
            string[] lines = tree.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            string child = string.Empty;
            var childTree = new StringBuilder();
            foreach (var line in lines) {
                if (line.StartsWith("├── ") || line.StartsWith("└── ")) {
                    if (child != string.Empty) {
                        this.children.Add(new FolderTree(childTree.ToString()));
                    }

                    child = line.Substring(4);
                    childTree = new StringBuilder(child);
                } else if (line.StartsWith("│   ") || line.StartsWith("    ")) {
                    childTree.Append(System.Environment.NewLine).Append(line.Substring(4));
                } else {
                    if (line.Contains(" {")) {
                        this.Name = line.Substring(0, line.IndexOf(" {"));
                        JObject jobject = JObject.Parse(line.Substring(line.IndexOf(" {")));
                        JToken value;
                        if (jobject.TryGetValue("lid", out value)) {
                            this.LocalId = value.ToString();
                        }

                        if (jobject.TryGetValue("rid", out value)) {
                            this.RemoteId = value.ToString();
                        }

                        if (jobject.TryGetValue("file", out value)) {
                            this.IsFile = (Boolean)value;
                        }
                    } else {
                        this.Name = line;
                    }
                }
            }

            string lastChild = childTree.ToString();
            if (!string.IsNullOrWhiteSpace(lastChild)) {
                this.children.Add(new FolderTree(lastChild));
            }

            this.children.Sort((FolderTree x, FolderTree y) => x.Name.CompareTo(y.Name));
        }

        public FolderTree(IFolder folder, string name = null) {
            this.Name = name ?? folder.Name;
            this.IsFile = false;
            this.RemoteId = folder.Id;
            foreach (var child in folder.GetChildren()) {
                if (child is IFolder) {
                    this.children.Add(new FolderTree(child as IFolder));
                } else if (child is IDocument) {
                    this.children.Add(new FolderTree(child as IDocument));
                }
            }

            this.children.Sort((FolderTree x, FolderTree y) => x.Name.CompareTo(y.Name));
        }

        public FolderTree(IDirectoryInfo dir, string name = null) {
            this.Name = name ?? dir.Name;
            this.IsFile = false;
            this.LocalId = dir.Uuid == null ? null : dir.Uuid.GetValueOrDefault().ToString();
            foreach (var child in dir.GetDirectories()) {
                this.children.Add(new FolderTree(child));
            }

            this.children.Sort((FolderTree x, FolderTree y) => x.Name.CompareTo(y.Name));
        }

        public FolderTree(DirectoryInfo dir, string name = null) : this(new DirectoryInfoWrapper(dir), name) {
        }

        private FolderTree(IDocument doc) {
            this.Name = doc.Name;
            this.IsFile = true;
            this.RemoteId = doc.Id;
        }

        private FolderTree(IFileInfo file) {
            this.Name = file.Name;
            this.IsFile = true;
            this.LocalId = file.Uuid == null ? null : file.Uuid.GetValueOrDefault().ToString();
        }

        private FolderTree(FileInfo file) : this(new FileInfoWrapper(file)) {
        }

        public string Name { get; private set; }

        public string LocalId { get; set; }

        public string RemoteId { get; set; }

        public bool IsFile { get; private set; }

        public override string ToString() {
            var tree = new StringBuilder();
            JObject json = new JObject();
            if (this.IsFile) {
                json.Add("file", this.IsFile);
            }

            if (!(string.IsNullOrEmpty(this.LocalId) && string.IsNullOrEmpty(this.RemoteId))) {
                json.Add("lid", this.LocalId);
                json.Add("rid", this.RemoteId);
            }

            var attributes = string.Format(" {0}", json.ToString(Newtonsoft.Json.Formatting.None));
            tree.AppendLine(string.Format("{0}{1}", this.Name, json.Properties().Count() == 0 ? string.Empty : attributes));
            int count = this.children.Count;
            foreach (var child in this.children) {
                string prefix = "│   ";
                if (count <= 1) {
                    tree.Append("└── ");
                    prefix = "    ";
                } else {
                    tree.Append("├── ");
                }

                string[] subTreeLines = child.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
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

        public override bool Equals(object obj) {
            if (obj != null && (obj is FolderTree || obj is string)) {
                var otherTree = obj is string ? new FolderTree((string)obj) : obj as FolderTree;
                if (!string.IsNullOrEmpty(this.LocalId) && !string.IsNullOrEmpty(otherTree.LocalId)) {
                    if (!this.LocalId.Equals(otherTree.LocalId)) {
                        return false;
                    }
                }

                if (!string.IsNullOrEmpty(this.RemoteId) && !string.IsNullOrEmpty(otherTree.RemoteId)) {
                    if (!this.RemoteId.Equals(otherTree.RemoteId)) {
                        return false;
                    }
                }

                if (!this.Name.Equals(otherTree.Name)) {
                    return false;
                }

                if (this.IsFile != otherTree.IsFile) {
                    return false;
                }

                if (this.children.Count != otherTree.children.Count) {
                    return false;
                }

                return this.children.SequenceEqual(otherTree.children);
            } else {
                return false;
            }
        }

        public static implicit operator string(FolderTree tree) {
            return tree == null ? null : tree.ToString();
        }

        public static implicit operator FolderTree(string tree) {
            return tree == null ? null : new FolderTree(tree);
        }

        public override int GetHashCode() {
            return this.ToString().GetHashCode();
        }
    }
}