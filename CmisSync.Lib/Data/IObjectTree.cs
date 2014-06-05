//-----------------------------------------------------------------------
// <copyright file="IObjectTree.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Data
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Object Tree interface. The tree has got the possibility to flag each node.
    /// </summary>
    /// <typeparam name="T">Type of the saved item.</typeparam>
    public interface IObjectTree<T>
    {
        /// <summary>
        /// Gets the item.
        /// </summary>
        /// <value>The item.</value>
        T Item { get; }

        /// <summary>
        /// Gets the sub trees as list of the child nodes.
        /// </summary>
        /// <value>The children.</value>
        IList<IObjectTree<T>> Children { get; }

        /// <summary>
        /// Returns a list with all items of the whole tree.
        /// </summary>
        /// <returns>The list.</returns>
        List<T> ToList();
    }
}