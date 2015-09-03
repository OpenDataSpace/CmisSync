//-----------------------------------------------------------------------
// <copyright file="MetaDataStorage.cs" company="GRAU DATA AG">
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
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    using CmisSync.Lib.PathMatcher;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DBreeze;
    using DBreeze.DataTypes;
    using DBreeze.Transactions;

    using log4net;

    /// <summary>
    /// Meta data storage.
    /// </summary>
    public class MetaDataStorage : IMetaDataStorage {
        private const string PropertyTable = "properties";
        private const string MappedObjectsTable = "objects";
        private const string MappedObjectsGuidsTable = "guids";
        private const string ChangeLogTokenKey = "ChangeLogToken";

        private static readonly ILog Logger = LogManager.GetLogger(typeof(MetaDataStorage));

        private readonly Regex slashRegex = new Regex(@"[/]{2,}", RegexOptions.None);

        /// <summary>
        /// The db engine.
        /// </summary>
        private DBreezeEngine engine;

        /// <summary>
        /// The path matcher.
        /// </summary>
        private IPathMatcher matcher;

        private bool fullValidationOnEachManipulation;

        static MetaDataStorage() {
            DBreezeInitializerSingleton.Init();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaDataStorage"/> class.
        /// </summary>
        /// <param name='engine'>
        /// DBreeze Engine. Must not be null.
        /// </param>
        /// <param name='matcher'>
        /// The Path matcher instance. Must not be null.
        /// </param>
        /// <param name='fullValidation'>
        /// Enables a complete DB validation after each db manipulation
        /// </param>
        /// <param name="disableInitialValidation">
        /// Disables initial validation of the object structure.
        /// </param>
        [CLSCompliant(false)]
        public MetaDataStorage(DBreezeEngine engine, IPathMatcher matcher, bool fullValidation, bool disableInitialValidation = false) {
            if (engine == null) {
                throw new ArgumentNullException("engine");
            }

            if (matcher == null) {
                throw new ArgumentNullException("matcher");
            }

            this.engine = engine;
            this.matcher = matcher;
            this.fullValidationOnEachManipulation = fullValidation;

            if (!disableInitialValidation) {
                try {
                    Logger.Debug("Starting DB Validation");
                    this.ValidateObjectStructure();
                    Logger.Debug("Finished DB Validation");
                } catch (InvalidDataException e) {
                    Logger.Fatal("Database object structure is invalid", e);
                }
            }
        }

        /// <summary>
        /// Gets the matcher.
        /// </summary>
        /// <value>
        /// The matcher.
        /// </value>
        public IPathMatcher Matcher {
            get {
                return this.matcher;
            }
        }

        /// <summary>
        /// Gets or sets the change log token that was stored at the end of the last successful CmisSync synchronization.
        /// </summary>
        /// <value>
        /// The change log token.
        /// </value>
        public string ChangeLogToken {
            get {
                using (var tran = this.engine.GetTransaction()) {
                    return tran.Select<string, string>(PropertyTable, ChangeLogTokenKey).Value;
                }
            }

            set {
                using (var tran = this.engine.GetTransaction()) {
                    tran.Insert<string, string>(PropertyTable, ChangeLogTokenKey, value);
                    tran.Commit();
                }
            }
        }

        /// <summary>
        /// Gets the object by passing the local path.
        /// </summary>
        /// <returns>
        /// The object saved for the local path or <c>null</c>
        /// </returns>
        /// <param name='path'>
        /// Local path from the saved object
        /// </param>
        public IMappedObject GetObjectByLocalPath(IFileSystemInfo path) {
            if (path == null) {
                throw new ArgumentNullException("path");
            }

            if (!this.matcher.CanCreateRemotePath(path.FullName)) {
                throw new ArgumentException(string.Format("Given path \"{0}\" is not able to be matched on remote path", path.FullName), "path");
            }

            using (var tran = this.engine.GetTransaction()) {
                string relativePath = this.matcher.GetRelativeLocalPath(path.FullName);
                List<string> pathSegments = new List<string>(relativePath.Split(Path.DirectorySeparatorChar));
                List<MappedObject> objects = new List<MappedObject>();
                foreach (var row in tran.SelectForward<string, DbCustomSerializer<MappedObject>>(MappedObjectsTable)) {
                    var value = row.Value;
                    if (value == null) {
                        continue;
                    }

                    var data = value.Get;
                    if (data == null) {
                        continue;
                    }

                    objects.Add(data);
                }

                MappedObject root = objects.Find(o => o.ParentId == null);
                if (root == null) {
                    return null;
                }

                if (root.Name != "/") {
                    if (root.Name == pathSegments[0]) {
                        pathSegments.RemoveAt(0);
                    } else {
                        return null;
                    }
                }

                MappedObject parent = root;
                foreach (var name in pathSegments) {
                    if (name.Equals(".")) {
                        continue;
                    }

                    MappedObject child = objects.Find(o => o.ParentId == parent.RemoteObjectId && o.Name == name);
                    if (child != null) {
                        parent = child;
                    } else {
                        return null;
                    }
                }

                return new MappedObject(parent);
            }
        }

        /// <summary>
        /// Gets the object by remote identifier.
        /// </summary>
        /// <returns>
        /// The saved object with the given remote identifier.
        /// </returns>
        /// <param name='id'>
        /// CMIS Object Id.
        /// </param>
        public IMappedObject GetObjectByRemoteId(string id) {
            using(var tran = this.engine.GetTransaction()) {
                DbCustomSerializer<MappedObject> value = tran.Select<string, DbCustomSerializer<MappedObject>>(MappedObjectsTable, id).Value;
                if (value != null) {
                    MappedObject data = value.Get;

                    if (data == null) {
                        return null;
                    }

                    return new MappedObject(data);
                }

                return null;
            }
        }

        /// <summary>
        /// Saves the mapped object.
        /// </summary>
        /// <param name='obj'>
        /// The MappedObject instance.
        /// </param>
        /// <exception cref="DublicateGuidException">Is thrown when guid already in database</exception>
        public void SaveMappedObject(IMappedObject obj) {
            string id = this.GetId(obj);
            using(var tran = this.engine.GetTransaction()) {
                var byteGuid = obj.Guid.ToByteArray();
                var row = tran.Select<byte[], string>(MappedObjectsGuidsTable, byteGuid);
                if (row.Exists && row.Value != id) {
                    tran.Rollback();
                    throw new DublicateGuidException(string.Format("An entry with Guid {0} already exists", obj.Guid));
                }

                if (this.fullValidationOnEachManipulation && obj.ParentId != null) {
                    DbCustomSerializer<MappedObject> value = tran.Select<string, DbCustomSerializer<MappedObject>>(MappedObjectsTable, obj.ParentId).Value;
                    if (value == null) {
                        tran.Rollback();
                        throw new InvalidDataException();
                    }
                }

                obj.LastTimeStoredInStorage = DateTime.UtcNow;
                tran.Insert<string, DbCustomSerializer<MappedObject>>(MappedObjectsTable, id, obj as MappedObject);
                if (!obj.Guid.Equals(Guid.Empty)) {
                    tran.Insert<byte[], string>(MappedObjectsGuidsTable, obj.Guid.ToByteArray(), id);
                }

                tran.Commit();
            }

            this.ValidateObjectStructureIfFullValidationIsEnabled();
        }

        /// <summary>
        /// Removes the saved object.
        /// </summary>
        /// <param name='obj'>
        /// Object to be removed.
        /// </param>
        public void RemoveObject(IMappedObject obj) {
            string id = this.GetId(obj);
            using (var tran = this.engine.GetTransaction()) {
                MappedObject root = null;
                List<MappedObject> objects = new List<MappedObject>();
                foreach (var row in tran.SelectForward<string, DbCustomSerializer<MappedObject>>(MappedObjectsTable)) {
                    var value = row.Value;
                    if (value == null) {
                        continue;
                    }

                    var data = value.Get;
                    if (data == null) {
                        continue;
                    }

                    if (row.Key.Equals(id)) {
                        root = data;
                    } else {
                        objects.Add(data);
                    }
                }

                if (root == null) {
                    return;
                }

                this.RemoveChildren(tran, root, ref objects);

                tran.RemoveKey<string>(MappedObjectsTable, id);
                tran.RemoveKey<byte[]>(MappedObjectsGuidsTable, root.Guid.ToByteArray());
                tran.Commit();
            }

            this.ValidateObjectStructureIfFullValidationIsEnabled();
        }

        /// <summary>
        /// Gets the remote path.
        /// </summary>
        /// <returns>The remote path.</returns>
        /// <param name='mappedObject'>The MappedObject instance.</param>
        public string GetRemotePath(IMappedObject mappedObject) {
            string id = this.GetId(mappedObject);
            using(var tran = this.engine.GetTransaction()) {
                string[] segments = this.GetRelativePathSegments(tran, id);
                StringBuilder pathBuilder = new StringBuilder(this.matcher.RemoteTargetRootPath);
                foreach (var name in segments) {
                    pathBuilder.Append("/").Append(name);
                }

                return this.slashRegex.Replace(pathBuilder.ToString(), @"/");
            }
        }

        /// <summary>
        /// Gets the local path. Return null if not exists.
        /// </summary>
        /// <returns>
        /// The local path.
        /// </returns>
        /// <param name='mappedObject'>
        /// Mapped object. Must not be null.
        /// </param>
        public string GetLocalPath(IMappedObject mappedObject) {
            string id = this.GetId(mappedObject);
            using(var tran = this.engine.GetTransaction()) {
                string[] segments = this.GetRelativePathSegments(tran, id);
                if (segments == null) {
                    return null;
                }

                if (segments.Length > 0 && segments[0].Equals("/")) {
                    string[] temp = new string[segments.Length - 1];
                    for (int i = 1; i < segments.Length; i++) {
                        temp[i - 1] = segments[i];
                    }

                    segments = temp;
                }

                return Path.Combine(this.matcher.LocalTargetRootPath, Path.Combine(segments));
            }
        }

        /// <summary>
        ///  Gets the children of the given parent object.
        /// </summary>
        /// <returns>
        ///  The saved children.
        /// </returns>
        /// <param name='parent'>
        ///  Parent of the children.
        /// </param>
        public List<IMappedObject> GetChildren(IMappedObject parent) {
            string parentId = this.GetId(parent);
            List<IMappedObject> results = new List<IMappedObject>();
            bool parentExists = false;
            using(var tran = this.engine.GetTransaction()) {
                foreach (var row in tran.SelectForward<string, DbCustomSerializer<MappedObject>>(MappedObjectsTable)) {
                    var data = row.Value.Get;
                    if (data == null) {
                        continue;
                    }

                    if (parentId == data.ParentId) {
                        results.Add(new MappedObject(data));
                    } else if(data.RemoteObjectId == parentId) {
                        parentExists = true;
                    }
                }
            }

            if (!parentExists) {
                throw new EntryNotFoundException();
            }

            return results;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Storage.FileSystem.MetaDataStorage"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Storage.FileSystem.MetaDataStorage"/>.</returns>
        public override string ToString() {
            string list = string.Empty;
            using (var tran = this.engine.GetTransaction()) {
                foreach (var row in tran.SelectForward<string, string>(MappedObjectsTable)) {
                    list += string.Format("[ Key={0}, Value={1}]{2}", row.Key, row.Value, Environment.NewLine);
                }
            }

            return string.Format("[MetaDataStorage: Matcher={0}, ChangeLogToken={1}]{2}{3}", this.Matcher, this.ChangeLogToken, Environment.NewLine, list);
        }

        /// <summary>
        /// Prints the file/folder structure like unix "find" command.
        /// </summary>
        /// <returns>The find string.</returns>
        public string ToFindString() {
            using(var tran = this.engine.GetTransaction()) {
                MappedObject root = null;
                List<MappedObject> objects = new List<MappedObject>();
                foreach (var row in tran.SelectForward<string, DbCustomSerializer<MappedObject>>(MappedObjectsTable)) {
                    var value = row.Value;
                    if (value == null) {
                        continue;
                    }

                    var data = value.Get;
                    if (data == null) {
                        continue;
                    }

                    if (data.ParentId == null) {
                        root = data;
                    } else {
                        objects.Add(data);
                    }
                }

                if (root == null) {
                    return string.Empty;
                }

                string result = this.PrintFindLines(objects, root, string.Empty);
                var sb = new StringBuilder();
                sb.Append(result);
                foreach (var obj in objects) {
                    sb.Append(Environment.NewLine).Append(obj.ToString());
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Validates the object structure.
        /// </summary>
        public void ValidateObjectStructure() {
            var objects = this.GetObjectList() ?? new List<IMappedObject>();
            var objectsDict = new Dictionary<string, IList<IMappedObject>>();
            IMappedObject root = null;
            foreach (var obj in objects) {
                if (obj.ParentId != null) {
                    if (!objectsDict.ContainsKey(obj.ParentId)) {
                        objectsDict[obj.ParentId] = new List<IMappedObject>();
                    }

                    objectsDict[obj.ParentId].Add(obj);
                } else {
                    root = obj;
                }
            }

            if (root == null) {
                if (objects.Count == 0) {
                    return;
                } else {
                    throw new InvalidDataException(
                        string.Format(
                        "root object is missing but {0} objects are stored",
                        objects.Count));
                }
            }

            this.RemoveChildrenRecursively(objectsDict, root);
            if (objectsDict.Count > 0) {
                var sb = new StringBuilder();
                foreach (var objs in objectsDict.Values) {
                    foreach (var obj in objs) {
                        sb.Append(obj).Append(Environment.NewLine);
                    }
                }

                throw new InvalidDataException(
                    string.Format(
                    "This objects are referencing to a not existing parentId: {0}{1}",
                    Environment.NewLine,
                    sb.ToString()));
            }
        }

        /// <summary>
        /// Gets the object by GUID.
        /// </summary>
        /// <returns>The object by GUID.</returns>
        /// <param name="guid">GUID of the requested object.</param>
        public IMappedObject GetObjectByGuid(Guid guid) {
            using (var tran = this.engine.GetTransaction()) {
                var row = tran.Select<byte[], string>(MappedObjectsGuidsTable, guid.ToByteArray());
                if (row.Exists) {
                    DbCustomSerializer<MappedObject> value = tran.Select<string, DbCustomSerializer<MappedObject>>(MappedObjectsTable, row.Value).Value;
                    if (value != null) {
                        MappedObject data = value.Get;

                        if (data == null) {
                            return null;
                        }

                        return new MappedObject(data);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a list of all mapped objects.
        /// </summary>
        /// <returns>The object list.</returns>
        public IList<IMappedObject> GetObjectList() {
            var objects = new List<IMappedObject>();
            using (var tran = this.engine.GetTransaction()) {
                foreach (var row in tran.SelectForward<string, DbCustomSerializer<MappedObject>>(MappedObjectsTable)) {
                    var value = row.Value;
                    if (value == null) {
                        continue;
                    }

                    var data = value.Get;
                    if (data == null) {
                        continue;
                    }

                    objects.Add(data);
                }
            }

            return objects.Count > 0 ? objects : null;
        }

        private IObjectTree<IMappedObject> GetSubTree(List<MappedObject> nodes, MappedObject parent) {
            var children = nodes.FindAll(o => o.ParentId == parent.RemoteObjectId);
            nodes.RemoveAll(o => o.ParentId == parent.RemoteObjectId);
            IList<IObjectTree<IMappedObject>> childNodes = new List<IObjectTree<IMappedObject>>();
            foreach (var child in children) {
                childNodes.Add(this.GetSubTree(nodes, child));
            }

            IObjectTree<IMappedObject> tree = new ObjectTree<IMappedObject> {
                Item = parent,
                Children = childNodes
            };
            return tree;
        }

        private void RemoveChildrenRecursively(IDictionary<string, IList<IMappedObject>> objects, IMappedObject root) {
            IList<IMappedObject> children;
            if (objects.TryGetValue(root.RemoteObjectId, out children)) {
                foreach (var child in children) {
                    if (child.Type == MappedObjectType.Folder) {
                        this.RemoveChildrenRecursively(objects, child);
                    }
                }
            }

            objects.Remove(root.RemoteObjectId);
        }

        private string PrintFindLines(List<MappedObject> objects, MappedObject parent, string prefix) {
            var sb = new StringBuilder();
            string path = Path.Combine(prefix, parent.Name);
            path = path.StartsWith("/") ? "." + path : path;
            sb.Append(path).Append(Environment.NewLine);
            List<MappedObject> children = objects.FindAll(o => o.ParentId == parent.RemoteObjectId);
            foreach (var child in children) {
                objects.Remove(child);
                sb.Append(this.PrintFindLines(objects, child, path));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the identifier of the given object and throws Exceptions if object or remote object id is null
        /// </summary>
        /// <returns>
        /// The identifier.
        /// </returns>
        /// <param name='obj'>
        /// Object with the containing remote id.
        /// </param>
        private string GetId(IMappedObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            string id = obj.RemoteObjectId;
            if (id == null) {
                throw new ArgumentException("The given object has no remote object id", "obj");
            }

            return id;
        }

        private string[] GetRelativePathSegments(Transaction tran, string id) {
            Stack<string> pathSegments = new Stack<string>();
            var value = tran.Select<string, DbCustomSerializer<MappedObject>>(MappedObjectsTable, id).Value;
            if (value == null) {
                return null;
            }

            MappedObject entry = value.Get;
            pathSegments.Push(entry.Name);
            while (entry.ParentId != null) {
                id = entry.ParentId;
                entry = tran.Select<string, DbCustomSerializer<MappedObject>>(MappedObjectsTable, id).Value.Get;
                pathSegments.Push(entry.Name);
            }

            return pathSegments.ToArray();
        }

        private void RemoveChildren(Transaction tran, MappedObject root, ref List<MappedObject> objects) {
            List<MappedObject> children = objects.FindAll(o => o.ParentId == root.RemoteObjectId);
            objects.RemoveAll(o => o.ParentId == root.RemoteObjectId);
            foreach (var child in children) {
                this.RemoveChildren(tran, child, ref objects);
                tran.RemoveKey<string>(MappedObjectsTable, child.RemoteObjectId);
                tran.RemoveKey<byte[]>(MappedObjectsGuidsTable, child.Guid.ToByteArray());
            }
        }

        private void ValidateObjectStructureIfFullValidationIsEnabled() {
            if (this.fullValidationOnEachManipulation) {
                this.ValidateObjectStructure();
            }
        }
    }
}