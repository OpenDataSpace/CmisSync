//-----------------------------------------------------------------------
// <copyright file="ObjectTree.cs" company="GRAU DATA AG">
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

    /// <summary>
    /// Object tree implementation.
    /// </summary>
    /// <typeparam name="T">Type of the stored node item.</typeparam>
    public class ObjectTree<T> : IObjectTree<T> {
        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        /// <value>The item.</value>
        public T Item { get; set; }

        /// <summary>
        /// Gets or sets the sub trees as list of the child nodes.
        /// </summary>
        /// <value>The children.</value>
        public IList<IObjectTree<T>> Children { get; set; }

        /// <summary>
        /// Returns a list with all items of the whole tree.
        /// </summary>
        /// <returns>The list.</returns>
        public List<T> ToList() {
            var list = new List<T>();
            if (this.Item != null) {
                list.Add(this.Item);
            }

            if (this.Children != null) {
                foreach (var child in this.Children) {
                    list.AddRange(child.ToList());
                }
            }

            return list;
        }
    }
}