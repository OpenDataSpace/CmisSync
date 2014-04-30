//-----------------------------------------------------------------------
// <copyright file="IMetaDataStorage.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Data;

    using DotCMIS.Client;

    /// <summary>
    /// I meta data storage.
    /// </summary>
    public interface IMetaDataStorage
    {
        /// <summary>
        /// Gets the matcher.
        /// </summary>
        /// <value>
        /// The matcher.
        /// </value>
        IPathMatcher Matcher { get; }

        /// <summary>
        /// Gets or sets the change log token that was stored at the end of the last successful CmisSync synchronization.
        /// </summary>
        /// <value>
        /// The change log token.
        /// </value>
        string ChangeLogToken { get; set; }

        /// <summary>
        /// Gets the object by passing the local path.
        /// </summary>
        /// <returns>
        /// The object saved for the local path or <c>null</c>
        /// </returns>
        /// <param name='path'>
        /// Local path from the saved object
        /// </param>
        IMappedObject GetObjectByLocalPath(IFileSystemInfo path);

        /// <summary>
        /// Gets the object by remote identifier.
        /// </summary>
        /// <returns>
        /// The saved object with the given remote identifier.
        /// </returns>
        /// <param name='id'>
        /// CMIS Object Id.
        /// </param>
        IMappedObject GetObjectByRemoteId(string id);

        /// <summary>
        /// Saves the mapped object.
        /// </summary>
        /// <param name='obj'>
        /// MappedObject instance.
        /// </param>
        void SaveMappedObject(IMappedObject obj);

        /// <summary>
        /// Removes the given object from Db
        /// </summary>
        /// <param name='obj'>
        /// Object with the Remote object id, which should be removed.
        /// </param>
        void RemoveObject(IMappedObject obj);

        /// <summary>
        /// Gets the remote path. Returns null if not exists.
        /// </summary>
        /// <returns>
        /// The remote path.
        /// </returns>
        /// <param name='mappedObject'>
        /// Mapped object. Must not be null.
        /// </param>
        string GetRemotePath(IMappedObject mappedObject);

        /// <summary>
        /// Gets the local path. Return null if not exists.
        /// </summary>
        /// <returns>
        /// The local path.
        /// </returns>
        /// <param name='mappedObject'>
        /// Mapped object. Must not be null.
        /// </param>
        string GetLocalPath(IMappedObject mappedObject);

        /// <summary>
        /// Gets the children of the given parent object.
        /// </summary>
        /// <returns>
        /// The saved children.
        /// </returns>
        /// <param name='parent'>
        /// Parent, which should be used to request its children.
        /// </param>
        List<IMappedObject> GetChildren(IMappedObject parent);
    }
}
