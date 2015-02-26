//-----------------------------------------------------------------------
// <copyright file="IIgnoredEntitiesCollection.cs" company="GRAU DATA AG">
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

    using DotCMIS.Client;

    /// <summary>
    /// Ignored entities collection.
    /// </summary>
    public interface IIgnoredEntitiesCollection {
        /// <summary>
        /// Add the specified ignored entity to collection.
        /// </summary>
        /// <param name="ignore">Ignored entity.</param>
        void Add(IIgnoredEntity ignore);

        /// <summary>
        /// Remove the specified ignored entity from collection.
        /// </summary>
        /// <param name="ignore">Ignored entity.</param>
        void Remove(IIgnoredEntity ignore);

        /// <summary>
        /// Remove the specified ignored entity with the given objectId from collection.
        /// </summary>
        /// <param name="objectId">Object identifier of the ignored entity which should be removed.</param>
        void Remove(string objectId);

        /// <summary>
        /// Determines whether the given Document is ignored.
        /// </summary>
        /// <returns><c>true</c> if the specified doc is ignored; otherwise, <c>false</c>.</returns>
        /// <param name="doc">Document to be checked.</param>
        IgnoredState IsIgnored(IDocument doc);

        /// <summary>
        /// Determines whether the given folder is ignored.
        /// </summary>
        /// <returns><c>true</c> if the given folder is ignored; otherwise, <c>false</c>.</returns>
        /// <param name="folder">Folder to be checked.</param>
        IgnoredState IsIgnored(IFolder folder);

        /// <summary>
        /// Determines whether the object with the given objectId is ignored.
        /// </summary>
        /// <returns><c>true</c> if the object with the given objectId is ignored; otherwise, <c>false</c>.</returns>
        /// <param name="objectId">Object identifier.</param>
        IgnoredState IsIgnoredId(string objectId);

        /// <summary>
        /// Determines whether this the ignored path is ignored.
        /// </summary>
        /// <returns><c>true</c> if the local path is ignored; otherwise, <c>false</c>.</returns>
        /// <param name="localPath">Local path.</param>
        IgnoredState IsIgnoredPath(string localPath);
    }
}