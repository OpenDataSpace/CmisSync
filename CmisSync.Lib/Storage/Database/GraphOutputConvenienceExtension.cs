//-----------------------------------------------------------------------
// <copyright file="GraphOutputConvenienceExtension.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Storage.Database {
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    /// <summary>
    /// Graph output convenience extension.
    /// </summary>
    public static class GraphOutputConvenienceExtension {
        /// <summary>
        /// Prints the tree to a file with given path.
        /// </summary>
        /// <param name="tree">Object tree.</param>
        /// <param name="path">Path to the target dot file.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void ToDotFile<T>(this IObjectTree<T> tree, string path) {
            tree.ToDotFile(new FileInfoWrapper(new FileInfo(path)));
        }

        /// <summary>
        /// Prints the list of objects to dot file.
        /// </summary>
        /// <param name="list">List of stored objects.</param>
        /// <param name="path">Path to the target dot file.</param>
        public static void ObjectListToDotFile(this IList<IMappedObject> list, string path) {
            list.ObjectListToDotFile(new FileInfoWrapper(new FileInfo(path)));
        }

        /// <summary>
        /// Prints the object tree to the given file info object.
        /// </summary>
        /// <param name="tree">Object tree.</param>
        /// <param name="file">File object where the content should be written to.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void ToDotFile<T>(this IObjectTree<T> tree, IFileInfo file) {
            if (file == null) {
                throw new ArgumentNullException("file");
            }

            using (var stream = file.Open(FileMode.CreateNew)) {
                tree.ToDotStream(stream);
            }
        }

        /// <summary>
        /// Prints the list of objects to dot file.
        /// </summary>
        /// <param name="list">Object list.</param>
        /// <param name="file">File object where the content should be written to.</param>
        public static void ObjectListToDotFile(this IList<IMappedObject> list, IFileInfo file) {
            if (file == null) {
                throw new ArgumentNullException("file");
            }

            using (var stream = file.Open(FileMode.CreateNew)) {
                list.ObjectListToDotStream(stream);
            }
        }

        /// <summary>
        /// Prints the object tree to the given outputstream.
        /// </summary>
        /// <param name="tree">Object tree.</param>
        /// <param name="outputsteam">Output steam.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void ToDotStream<T>(this IObjectTree<T> tree, Stream outputsteam) {
            using (var writer = new StreamWriter(outputsteam)) {
                writer.WriteLine("digraph tree {");
                writer.WriteLine("\tgraph [rankdir = \"LR\"];");
                tree.ToDotString(writer);
                writer.WriteLine("}");
            }
        }

        /// <summary>
        /// Prints the object list to the given outputstream.
        /// </summary>
        /// <param name="list">Object list.</param>
        /// <param name="outputsteam">Output steam.</param>
        public static void ObjectListToDotStream(this IList<IMappedObject> list, Stream outputsteam) {
            using (var writer = new StreamWriter(outputsteam)) {
                writer.WriteLine("digraph tree {");
                writer.WriteLine("\tgraph [rankdir = \"LR\"];");
                list.ObjectListToDotString(writer);
                writer.WriteLine("}");
            }
        }

        private static void ToDotString<T>(this IObjectTree<T> tree, StreamWriter writer) {
            DotTreeWriterFactory.CreateWriter<T>().ToDotString(tree, writer);
        }

        private static void ObjectListToDotString(this IList<IMappedObject> list, StreamWriter writer) {
            foreach (var entry in list) {
                var remoteId = entry.RemoteObjectId;
                writer.WriteLine(string.Format("\t\"{0}\" [label=\"{1}|<id>ObjectId: {2}|<uuid>UUID: {3}\", shape=record] ;", remoteId, entry.Name, remoteId, entry.Guid));
                if (entry.ParentId != null) {
                    writer.WriteLine(string.Format("\t\"{0}\" -> \"{1}\" ;", entry.ParentId, remoteId));
                }
            }
        }

        private sealed class DotTreeWriterFactory {
            public static IDotTreeWriter<T> CreateWriter<T>() {
                if (typeof(T) == typeof(IFileSystemInfo)) {
                    return new LocalTreeDotWriter<T>();
                }

                if (typeof(T) == typeof(IFileableCmisObject)) {
                    return new RemoteTreeDotWriter<T>();
                }

                if (typeof(T) == typeof(IMappedObject)) {
                    return new StoredTreeDotWriter<T>();
                }

                return new StringTreeDotWriter<T>();
            }
        }

        private sealed class StringTreeDotWriter<T> : IDotTreeWriter<T> {
            public void ToDotString(IObjectTree<T> tree, StreamWriter writer) {
                foreach (var child in tree.Children ?? new List<IObjectTree<T>>()) {
                    writer.Write("\t");
                    writer.Write(tree.Item.ToString());
                    writer.Write(" -> ");
                    writer.Write(child.Item.ToString());
                    writer.WriteLine(";");
                    child.ToDotString(writer);
                }
            }
        }

        private sealed class LocalTreeDotWriter<T> : IDotTreeWriter<T> {
            public void ToDotString(IObjectTree<T> tree, StreamWriter writer) {
                var t = tree as IObjectTree<IFileSystemInfo>;
                var item = t.Item;
                var fullName = item.FullName;
                var name = item.Name;
                var uuid = item.Uuid.GetValueOrDefault();
                writer.WriteLine(string.Format("\t\"{0}\" [label=\"{1}|<uuid>UUID: {2}\", shape=record] ;", fullName, name, uuid));
                foreach (var child in t.Children ?? new List<IObjectTree<IFileSystemInfo>>()) {
                    writer.WriteLine(string.Format("\t\"{0}\" -> \"{1}\" ;", fullName, child.Item.FullName));
                    child.ToDotString(writer);
                }
            }
        }

        private sealed class RemoteTreeDotWriter<T> : IDotTreeWriter<T> {
            public void ToDotString(IObjectTree<T> tree, StreamWriter writer) {
                var t = tree as IObjectTree<IFileableCmisObject>;
                var item = t.Item;
                var name = item.Name;
                var id = item.Id;
                writer.WriteLine(string.Format("\t\"{0}\" [label=\"{1}|<id>ObjectId: {2}\", shape=record] ;", id, name, id));
                foreach (var child in t.Children ?? new List<IObjectTree<IFileableCmisObject>>()) {
                    writer.WriteLine(string.Format("\t\"{0}\" -> \"{1}\" ;", id, child.Item.Id));
                    child.ToDotString(writer);
                }
            }
        }

        private sealed class StoredTreeDotWriter<T> : IDotTreeWriter<T> {
            public void ToDotString(IObjectTree<T> tree, StreamWriter writer) {
                var t = tree as IObjectTree<IMappedObject>;
                var item = t.Item;
                var uuid = item.Guid;
                var id = item.RemoteObjectId;
                var name = item.Name;
                writer.WriteLine(string.Format("\t\"{0}\" [label=\"{1}|<id>ObjectId: {2}|<uuid>UUID: {3}\", shape=record] ;", id, name, id, uuid));
                foreach (var child in t.Children ?? new List<IObjectTree<IMappedObject>>()) {
                    writer.WriteLine(string.Format("\t\"{0}\" -> \"{1}\" ;", id, child.Item.RemoteObjectId));
                    child.ToDotString(writer);
                }
            }
        }
    }
}