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

    using CmisSync.Lib.Data;

    using DBreeze;
    using DBreeze.DataTypes;

    /// <summary>
    /// Meta data storage.
    /// </summary>
    public class MetaDataStorage : IMetaDataStorage
    {
        /// <summary>
        /// The db engine.
        /// </summary>
        private DBreezeEngine engine = null;

        /// <summary>
        /// The path matcher.
        /// </summary>
        private IPathMatcher matcher = null;

        private static readonly string PropertyTable = "properties";
        private static readonly string FileTable = "files";
        private static readonly string FolderTable = "folder";
        private static readonly string ChangeLogTokenKey = "ChangeLogToken";
        private static readonly string LocalPathKey = "LocalPath";
        private static readonly string RemotePathKey = "RemotePath";

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Storage.MetaDataStorage"/> class.
        /// </summary>
        /// <param name='engine'>
        /// DBreeze Engine. Must not be null.
        /// </param>
        /// <param name='matcher'>
        /// The Path matcher instance. Must not be null.
        /// </param>
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
            throw new NotImplementedException();
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
                AbstractMappedObject obj = tran.Select<string, DbCustomSerializer<MappedFile>>(FileTable, id).Value.Get;
                return (obj == null) ? tran.Select<string, DbCustomSerializer<MappedFolder>>(FolderTable, id).Value.Get : obj;
            }
        }

        /// <summary>
        /// Saves the mapped file.
        /// </summary>
        /// <param name='file'>
        /// Mapped File instance.
        /// </param>
        public void SaveMappedFile(IMappedFile file)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves the mapped folder.
        /// </summary>
        /// <param name='folder'>
        /// Mapped Folder instance.
        /// </param>
        public void SaveMappedFolder(IMappedFolder folder)
        {
            throw new NotImplementedException();
        }
    }
}
