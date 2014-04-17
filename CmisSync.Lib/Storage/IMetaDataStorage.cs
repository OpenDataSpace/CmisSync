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
        /// Saves the mapped file.
        /// </summary>
        /// <param name='file'>
        /// Mapped File instance.
        /// </param>
        void SaveMappedFile(IMappedFile file);

        /// <summary>
        /// Saves the mapped folder.
        /// </summary>
        /// <param name='folder'>
        /// Mapped Folder instance.
        /// </param>
        void SaveMappedFolder(IMappedFolder folder);

        /// <summary>
        /// Removes the given object from Db
        /// </summary>
        /// <param name='obj'>
        /// Object with the Remote object id, which should be removed.
        /// </param>
        void RemoveObject(IMappedObject obj);
    }
}
