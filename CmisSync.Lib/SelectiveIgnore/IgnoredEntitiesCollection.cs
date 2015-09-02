//-----------------------------------------------------------------------
// <copyright file="IgnoredEntitiesCollection.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.SelectiveIgnore {
    using System;
    using System.Collections.Generic;
    using System.IO;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    /// <summary>
    /// Ignored entities collection implementation.
    /// </summary>
    public class IgnoredEntitiesCollection : IIgnoredEntitiesCollection {
        private Dictionary<string, IIgnoredEntity> entries = new Dictionary<string, IIgnoredEntity>();

        /// <summary>
        /// Add the specified ignored entity to collection.
        /// </summary>
        /// <param name="ignore">Ignored entity.</param>
        public void Add(IIgnoredEntity ignore) {
            if (ignore == null) {
                throw new ArgumentNullException("ignore");
            }

            this.entries[ignore.ObjectId] = ignore;
        }

        /// <summary>
        /// Remove the specified ignored entity from collection.
        /// </summary>
        /// <param name="ignore">Ignored entity.</param>
        public void Remove(IIgnoredEntity ignore) {
            if (ignore == null) {
                throw new ArgumentNullException("ignore");
            }

            this.entries.Remove(ignore.ObjectId);
        }

        /// <summary>
        /// Remove the specified ignored entity from collection by using remote id.
        /// </summary>
        /// <param name="objectId">Object identifier.</param>
        public void Remove(string objectId) {
            this.entries.Remove(objectId);
        }

        /// <summary>
        /// Determines whether the given Document is ignored.
        /// </summary>
        /// <returns>true</returns>
        /// <c>false</c>
        /// <param name="doc">Document to be checked.</param>
        public IgnoredState IsIgnored(IDocument doc) {
            if (doc == null) {
                throw new ArgumentNullException("doc");
            }

            if (this.IsIgnoredId(doc.Id) == IgnoredState.Ignored) {
                return IgnoredState.Ignored;
            } else {
                if (doc.Parents != null && doc.Parents.Count > 0) {
                    if (this.IsIgnored(doc.Parents[0]) != IgnoredState.NotIgnored) {
                        return IgnoredState.Inherited;
                    }
                }
            }

            return IgnoredState.NotIgnored;
        }

        /// <summary>
        /// Determines whether the given folder is ignored.
        /// </summary>
        /// <returns>true</returns>
        /// <c>false</c>
        /// <param name="folder">Folder to be checked.</param>
        public IgnoredState IsIgnored(IFolder folder) {
            if (folder == null) {
                throw new ArgumentNullException("folder");
            }

            if (this.IsIgnoredId(folder.Id) == IgnoredState.Ignored) {
                return IgnoredState.Ignored;
            } else {
                try {
                    var parent = folder.FolderParent;
                    if (parent != null) {
                        if (this.IsIgnored(parent) != IgnoredState.NotIgnored) {
                            return IgnoredState.Inherited;
                        }
                    }
                } catch (CmisObjectNotFoundException) {
                    return IgnoredState.NotIgnored;
                }
            }

            return IgnoredState.NotIgnored;
        }

        /// <summary>
        /// Determines whether the object with the given objectId is ignored.
        /// </summary>
        /// <returns>true</returns>
        /// <c>false</c>
        /// <param name="objectId">Object identifier.</param>
        public IgnoredState IsIgnoredId(string objectId) {
            if (this.entries.ContainsKey(objectId)) {
                return IgnoredState.Ignored;
            } else {
                return IgnoredState.NotIgnored;
            }
        }

        /// <summary>
        /// Determines whether this the ignored path is ignored.
        /// </summary>
        /// <returns>true</returns>
        /// <c>false</c>
        /// <param name="localPath">Local path.</param>
        public IgnoredState IsIgnoredPath(string localPath) {
            foreach (var entry in this.entries.Values) {
                if (localPath == entry.LocalPath) {
                    return IgnoredState.Ignored;
                } else if (localPath.StartsWith(entry.LocalPath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? entry.LocalPath : entry.LocalPath + Path.DirectorySeparatorChar.ToString())) {
                    return IgnoredState.Inherited;
                }
            }

            return IgnoredState.NotIgnored;
        }
    }
}