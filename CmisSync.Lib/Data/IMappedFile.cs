using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;

using CmisSync.Lib.Storage;

using DotCMIS.Client;

namespace CmisSync.Lib.Data
{
    public interface IMappedFile : IMappedObject
    {
        string GetLocalPath ();

        string GetRemotePath ();

        string GetLocalPath (IMappedFolder parent);

        string GetRemotePath (IMappedFolder parent);

        bool ExistsLocally (IMappedFolder parent);
    }

}

