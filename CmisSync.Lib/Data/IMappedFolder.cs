using System;
using System.IO;
using System.Collections.Generic;

using CmisSync.Lib.Storage;

namespace CmisSync.Lib.Data
{
    public interface IMappedFolder : IMappedObject
    {
        string GetLocalPath ();

        string GetRemotePath ();

        IMappedFolder Parent { get; set; }

        List<IMappedObject> Children { get; set; }

    }


}

