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

namespace CmisSync.Lib.Storage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using CmisSync.Lib.Data;

    using DBreeze;
    using DBreeze.DataTypes;
    using DBreeze.Transactions;

    /// <summary>
    /// Meta data storage.
    /// </summary>
    public class MetaDataStorage : IMetaDataStorage
    {
        private static readonly string PropertyTable = "properties";
        private static readonly string MappedObjectsTable = "objects";
        private static readonly string ChangeLogTokenKey = "ChangeLogToken";

        /// <summary>
        /// The db engine.
        /// </summary>
        private DBreezeEngine engine = null;

        /// <summary>
        /// The path matcher.
        /// </summary>
        private IPathMatcher matcher = null;

        static MetaDataStorage()
        {
            DBreezeInitializerSingleton.Init();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Storage.MetaDataStorage"/> class.
        /// </summary>
        /// <param name='engine'>
        /// DBreeze Engine. Must not be null.
        /// </param>
        /// <param name='matcher'>
        /// The Path matcher instance. Must not be null.
        /// </param>
        [CLSCompliant(false)]
        public MetaDataStorage(DBreezeEngine engine, IPathMatcher matcher)
        {
            if (engine == null)
            {
                throw new ArgumentNullException("Given DBreeze engine instance is null");
            }

            if (matcher == null)
            {
                throw new ArgumentNullException("Given Matcher is null");
            }

            this.engine = engine;
            this.matcher = matcher;
        }

        /// <summary>
        /// Gets the matcher.
        /// </summary>
        /// <value>
        /// The matcher.
        /// </value>
        public IPathMatcher Matcher
        {
            get
            {
                return this.matcher;
            }
        }

        /// <summary>
        /// Gets or sets the change log token that was stored at the end of the last successful CmisSync synchronization.
        /// </summary>
        /// <value>
        /// The change log token.
        /// </value>
        public string ChangeLogToken
        {
            get
            {
                using (var tran = this.engine.GetTransaction())
                {
                    return tran.Select<string, string>(PropertyTable, ChangeLogTokenKey).Value;
                }
            }

            set
            {
                using (var tran = this.engine.GetTransaction())
                {
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
        public IMappedObject GetObjectByLocalPath(IFileSystemInfo path)
        {
            if(path == null)
            {
                throw new ArgumentNullException("Given path is null");
            }

            if(!this.matcher.CanCreateRemotePath(path.FullName))
            {
                throw new ArgumentException(string.Format("Given path \"{0}\" is not able to be matched on remote path", path.FullName));
            }

            using(var tran = this.engine.GetTransaction())
            {
                string relativePath = this.matcher.GetRelativeLocalPath(path.FullName);
                List<string> pathSegments = new List<string>(relativePath.Split(Path.DirectorySeparatorChar));
                List<MappedObject> objects = new List<MappedObject>();
                foreach (var row in tran.SelectForward<string, DbCustomSerializer<MappedObject>>(MappedObjectsTable))
                {
                    var value = row.Value;
                    if(value == null)
                    {
                        continue;
                    }

                    var data = value.Get;
                    if(data == null)
                    {
                        continue;
                    }

                    objects.Add(data);
                }

                MappedObject root = objects.Find(o => o.ParentId == null);
                if(root == null)
                {
                    return null;
                }

                if(root.Name != "/")
                {
                    if(root.Name == pathSegments[0])
                    {
                        pathSegments.RemoveAt(0);
                    }
                    else
                    {
                        return null;
                    }
                }

                MappedObject parent = root;
                foreach(var name in pathSegments)
                {
                    if(name.Equals(".")) {
                        continue;
                    }

                    MappedObject child = objects.Find(o => o.ParentId == parent.RemoteObjectId && o.Name == name);
                    if(child != null)
                    {
                        parent = child;
                    }
                    else
                    {
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
        public IMappedObject GetObjectByRemoteId(string id)
        {
            using(var tran = this.engine.GetTransaction())
            {
                DbCustomSerializer<MappedObject> value = tran.Select<string, DbCustomSerializer<MappedObject>>(MappedObjectsTable, id).Value;
                if(value != null)
                {
                    MappedObject data = value.Get;

                    if (data == null)
                    {
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
        public void SaveMappedObject(IMappedObject obj)
        {
            string id = this.GetId(obj);
            using(var tran = this.engine.GetTransaction())
            {
                tran.Insert<string, DbCustomSerializer<MappedObject>>(MappedObjectsTable, id, obj as MappedObject);
                tran.Commit();
            }
        }

        /// <summary>
        /// Removes the saved object.
        /// </summary>
        /// <param name='obj'>
        /// Object to be removed.
        /// </param>
        public void RemoveObject(IMappedObject obj)
        {
            string id = this.GetId(obj);
            using(var tran = this.engine.GetTransaction())
            {
                MappedObject root = null;
                List<MappedObject> objects = new List<MappedObject>();
                foreach (var row in tran.SelectForward<string, DbCustomSerializer<MappedObject>>(MappedObjectsTable))
                {
                    var value = row.Value;
                    if(value == null)
                    {
                        continue;
                    }

                    var data = value.Get;
                    if(data == null)
                    {
                        continue;
                    }

                    if(row.Key.Equals(id))
                    {
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
                tran.Commit();
            }
        }

        /// <summary>
        /// Gets the remote path.
        /// </summary>
        /// <returns>
        /// The remote path.
        /// </returns>
        /// <param name='obj'>
        /// The MappedObject instance.
        /// </param>
        public string GetRemotePath(IMappedObject obj)
        {
            string id = this.GetId(obj);
            using(var tran = this.engine.GetTransaction())
            {
                string[] segments = this.GetRelativePathSegments(tran, id);
                string path = this.matcher.RemoteTargetRootPath;
                foreach(var name in segments)
                {
                    path += name.StartsWith("/") ? name : "/" + name;
                }

                return path.Replace("//", "/");
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
        public string GetLocalPath(IMappedObject mappedObject)
        {
            string id = this.GetId(mappedObject);
            using(var tran = this.engine.GetTransaction())
            {
                string[] segments = this.GetRelativePathSegments(tran, id);
                if(segments.Length > 0 && segments[0].Equals("/")) {
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
        public List<IMappedObject> GetChildren(IMappedObject parent)
        {
            string parentId = this.GetId(parent);
            List<IMappedObject> results = new List<IMappedObject>();
            bool parentExists = false;
            using(var tran = this.engine.GetTransaction())
            {
                foreach (var row in tran.SelectForward<string, DbCustomSerializer<MappedObject>>(MappedObjectsTable))
                {
                    var data = row.Value.Get;
                    if(data == null)
                    {
                        continue;
                    }

                    if(parentId == data.ParentId)
                    {
                        results.Add(new MappedObject(data));
                    }
                    else if(data.RemoteObjectId == parentId)
                    {
                        parentExists = true;
                    }
                }
            }

            if(!parentExists)
            {
                throw new EntryNotFoundException();
            }

            return results;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Storage.MetaDataStorage"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Storage.MetaDataStorage"/>.</returns>
        public override string ToString()
        {
            string list = string.Empty;
            using (var tran = this.engine.GetTransaction())
            {
                foreach (var row in tran.SelectForward<string, string>(MappedObjectsTable))
                {
                    list += string.Format("[ Key={0}, Value={1}]{2}", row.Key, row.Value, Environment.NewLine);
                }
            }

            return string.Format("[MetaDataStorage: Matcher={0}, ChangeLogToken={1}]{2}{3}", this.Matcher, this.ChangeLogToken, Environment.NewLine, list);
        }

        /// <summary>
        /// Prints the file/folder structure like unix "find" command.
        /// </summary>
        /// <returns>The find string.</returns>
        public string ToFindString()
        {
            using(var tran = this.engine.GetTransaction())
            {
                MappedObject root = null;
                List<MappedObject> objects = new List<MappedObject>();
                foreach (var row in tran.SelectForward<string, DbCustomSerializer<MappedObject>>(MappedObjectsTable))
                {
                    var value = row.Value;
                    if(value == null)
                    {
                        continue;
                    }

                    var data = value.Get;
                    if(data == null)
                    {
                        continue;
                    }

                    if(data.ParentId == null)
                    {
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
                foreach(var obj in objects) {
                    sb.Append(Environment.NewLine).Append(obj.ToString());
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Validates the object structure.
        /// </summary>
        public void ValidateObjectStructure() {
            MappedObject root = null;
            List<MappedObject> objects = new List<MappedObject>();
            using(var tran = this.engine.GetTransaction())
            {
                foreach (var row in tran.SelectForward<string, DbCustomSerializer<MappedObject>>(MappedObjectsTable))
                {
                    var value = row.Value;
                    if(value == null)
                    {
                        continue;
                    }

                    var data = value.Get;
                    if(data == null)
                    {
                        continue;
                    }

                    if(data.ParentId == null)
                    {
                        root = data;
                    } else {
                        objects.Add(data);
                    }
                }
            }

            if (root == null) {
                if (objects.Count == 0) {
                    return;
                } else {
                    throw new InvalidDataException(
                        string.Format(
                        "root object is missing but {0} objects are stored{1}{2}",
                        objects.Count,
                        Environment.NewLine,
                        this.ToString()));
                }
            }

            this.RemoveChildrenRecursively(objects, root);
            if (objects.Count > 0) {
                var sb = new StringBuilder();
                foreach(var obj in objects) {
                    sb.Append(obj).Append(Environment.NewLine);
                }

                throw new InvalidDataException(
                    string.Format(
                    "This objects are referencing to a not existing parentId: {0}{1}{0}{2}",
                    Environment.NewLine,
                    sb.ToString(),
                    this.ToString()));
            }
        }

        /// <summary>
        /// Gets the object by GUID.
        /// </summary>
        /// <returns>The object by GUID.</returns>
        /// <param name="guid">GUID of the requested object.</param>
        public IMappedObject GetObjectByGuid(Guid guid)
        {
            using(var tran = this.engine.GetTransaction())
            {
                foreach (var row in tran.SelectForward<string, DbCustomSerializer<MappedObject>>(MappedObjectsTable))
                {
                    var value = row.Value;
                    if (value == null) {
                        continue;
                    }

                    var data = value.Get;
                    if (data == null) {
                        continue;
                    }

                    if (data.Guid == guid) {
                        return data;
                    }
                }
            }

            return null;
        }

        private void RemoveChildrenRecursively(List<MappedObject> objects, MappedObject root)
        {
            var children = objects.FindAll(o => o.ParentId == root.RemoteObjectId);
            foreach (var child in children) {
                objects.Remove(child);
                if (child.Type == MappedObjectType.Folder) {
                    this.RemoveChildrenRecursively(objects, child);
                }
            }
        }

        private string PrintFindLines(List<MappedObject> objects, MappedObject parent, string prefix) {
            var sb = new StringBuilder();
            string path = Path.Combine(prefix, parent.Name);
            path = path.StartsWith("/") ? "." + path : path;
            sb.Append(path).Append(Environment.NewLine);
            List<MappedObject> children = objects.FindAll(o => o.ParentId == parent.RemoteObjectId);
            foreach(var child in children) {
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
        private string GetId(IMappedObject obj)
        {
            if(obj == null)
            {
                throw new ArgumentNullException("The given obj is null");
            }

            string id = obj.RemoteObjectId;
            if(id == null)
            {
                throw new ArgumentException("The given object has no remote object id");
            }

            return id;
        }

        private string[] GetRelativePathSegments(Transaction tran, string id)
        {
            Stack<string> pathSegments = new Stack<string>();
            MappedObject entry = tran.Select<string, DbCustomSerializer<MappedObject>>(MappedObjectsTable, id).Value.Get;
            pathSegments.Push(entry.Name);
            while(entry.ParentId != null)
            {
                id = entry.ParentId;
                entry = tran.Select<string, DbCustomSerializer<MappedObject>>(MappedObjectsTable, id).Value.Get;
                pathSegments.Push(entry.Name);
            }

            return pathSegments.ToArray();
        }

        private void RemoveChildren(Transaction tran, MappedObject root, ref List<MappedObject> objects)
        {
            List<MappedObject> children = objects.FindAll(o => o.ParentId == root.RemoteObjectId);
            objects.RemoveAll(o => o.ParentId == root.RemoteObjectId);
            foreach(var child in children) {
                this.RemoveChildren(tran, child, ref objects);
                tran.RemoveKey<string>(MappedObjectsTable, child.RemoteObjectId);
            }
        }
    }
}
