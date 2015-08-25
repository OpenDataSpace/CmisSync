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

        public void Add(IIgnoredEntity ignore) {
            this.entries[ignore.ObjectId] = ignore;
        }

        public void Remove(IIgnoredEntity ignore) {
            this.entries.Remove(ignore.ObjectId);
        }

        public void Remove(string objectId) {
            this.entries.Remove(objectId);
        }

        public IgnoredState IsIgnored(IDocument doc) {
            if (doc == null) {
                throw new ArgumentNullException("doc");
            }

            if (this.IsIgnoredId(doc.Id) == IgnoredState.IGNORED) {
                return IgnoredState.IGNORED;
            } else {
                if (doc.Parents != null && doc.Parents.Count > 0) {
                    if (this.IsIgnored(doc.Parents[0]) != IgnoredState.NOT_IGNORED) {
                        return IgnoredState.INHERITED;
                    }
                }
            }

            return IgnoredState.NOT_IGNORED;
        }

        public IgnoredState IsIgnored(IFolder folder) {
            if (folder == null) {
                throw new ArgumentNullException("folder");
            }

            if (this.IsIgnoredId(folder.Id) == IgnoredState.IGNORED) {
                return IgnoredState.IGNORED;
            } else {
                try {
                    var parent = folder.FolderParent;
                    if (parent != null) {
                        if (this.IsIgnored(parent) != IgnoredState.NOT_IGNORED) {
                            return IgnoredState.INHERITED;
                        }
                    }
                } catch (CmisObjectNotFoundException) {
                    return IgnoredState.NOT_IGNORED;
                }
            }

            return IgnoredState.NOT_IGNORED;
        }

        public IgnoredState IsIgnoredId(string objectId) {
            if (this.entries.ContainsKey(objectId)) {
                return IgnoredState.IGNORED;
            } else {
                return IgnoredState.NOT_IGNORED;
            }
        }

        public IgnoredState IsIgnoredPath(string localPath) {
            foreach (var entry in this.entries.Values) {
                if (localPath == entry.LocalPath) {
                    return IgnoredState.IGNORED;
                } else if (localPath.StartsWith(entry.LocalPath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? entry.LocalPath : entry.LocalPath + Path.DirectorySeparatorChar.ToString())) {
                    return IgnoredState.INHERITED;
                }
            }

            return IgnoredState.NOT_IGNORED;
        }
    }
}