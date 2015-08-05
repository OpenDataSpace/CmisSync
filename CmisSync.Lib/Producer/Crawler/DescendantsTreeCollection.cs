//-----------------------------------------------------------------------
// <copyright file="DescendantsTreeCollection.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Producer.Crawler {
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    /// <summary>
    /// Descendants tree collection.
    /// </summary>
    /// <exception cref='ArgumentNullException'>
    /// <attribution license="cc4" from="Microsoft" modified="false" /><para>The exception that is thrown when a null
    /// reference (Nothing in Visual Basic) is passed to a method that does not accept it as a valid argument. </para>
    /// </exception>
    public struct DescendantsTreeCollection {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Producer.Crawler.DescendantsTreeCollection"/> struct.
        /// </summary>
        /// <param name='storedTree'>
        /// Stored tree.
        /// </param>
        /// <param name='localTree'>
        /// Local tree.
        /// </param>
        /// <param name='remoteTree'>
        /// Remote tree.
        /// </param>
        /// <exception cref='ArgumentNullException'>
        /// <attribution license="cc4" from="Microsoft" modified="false" /><para>The exception that is thrown when a
        /// null reference (Nothing in Visual Basic) is passed to a method that does not accept it as a valid argument. </para>
        /// </exception>
        public DescendantsTreeCollection(IList<IMappedObject> storedObjects, IObjectTree<IFileSystemInfo> localTree, IObjectTree<IFileableCmisObject> remoteTree) : this() {
            if (storedObjects == null) {
                throw new ArgumentNullException("storedObjects");
            }

            if (localTree == null) {
                throw new ArgumentNullException("localTree");
            }

            if (remoteTree == null) {
                throw new ArgumentNullException("remoteTree");
            }

            this.StoredObjects = storedObjects;
            this.LocalTree = localTree;
            this.RemoteTree = remoteTree;
        }

        /// <summary>
        /// Gets the stored tree.
        /// </summary>
        /// <value>
        /// The stored tree.
        /// </value>
        public IList<IMappedObject> StoredObjects { get; private set; }

        /// <summary>
        /// Gets the local tree.
        /// </summary>
        /// <value>
        /// The local tree.
        /// </value>
        public IObjectTree<IFileSystemInfo> LocalTree { get; private set; }

        /// <summary>
        /// Gets the remote tree.
        /// </summary>
        /// <value>
        /// The remote tree.
        /// </value>
        public IObjectTree<IFileableCmisObject> RemoteTree { get; private set; }
    }
}