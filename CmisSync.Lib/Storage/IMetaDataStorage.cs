using System;
using System.IO;
using System.Collections.Generic;

using DotCMIS.Client;

using CmisSync.Lib.Data;

namespace CmisSync.Lib.Storage
{
    public interface IMetaDataStorage
    {


        IPathMatcher Matcher { get; }

        /// <summary>
        /// Get the time at which the file was last modified.
        /// This is the time on the CMIS server side, in UTC. Client-side time does not matter.
        /// </summary>
        DateTime? GetServerSideModificationDate(string path);

        /// <summary>
        /// Checks whether the storage contains a given file.
        /// </summary>
        bool ContainsFile(string path);

        /// <summary>
        /// <returns>path field in files table for <paramref name="id"/></returns>
        /// </summary>
        string GetFilePath(string id);

        /// <summary>
        /// Checks whether the storage contains a given folder.
        /// </summary>
        bool ContainsFolder(string path);

        /// <summary>
        /// Returns id for given Path
        /// </summary>
        string GetFileId(string path);

        /// <summary>
        /// Returns id for given Path
        /// </summary>
        string GetFolderId(string path);

        /// <summary>
        /// <returns>path field in folders table for <paramref name="id"/></returns>
        /// </summary>
        string GetFolderPath(string id);


        /// <summary>
        /// Get the ChangeLog token that was stored at the end of the last successful CmisSync synchronization.
        /// </summary>
        string GetChangeLogToken();


        /// <summary>
        /// Set the stored ChangeLog token.
        /// This should be called after each successful CmisSync synchronization.
        /// </summary>
        void SetChangeLogToken(string token);


    }
}

