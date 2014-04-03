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
        /// Get and Sets the ChangeLog token that was stored at the end of the last successful CmisSync synchronization.
        /// </summary>
        string ChangeLogToken { get; set; }

        IMappedObject GetObjectByLocalPath(IFileSystemInfo path);

        IMappedObject GetObjectByRemoteId(string id);
    }
}

