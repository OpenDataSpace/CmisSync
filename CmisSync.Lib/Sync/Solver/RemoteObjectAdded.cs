using System;

using System.IO;

using DotCMIS.Client;

using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;

namespace CmisSync.Lib.Sync.Solver
{
    using log4net;

    public class RemoteObjectAdded : ISolver
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RemoteObjectAdded));

        public virtual void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFile, IObjectId remoteId){
            Logger.Debug(localFile.FullName);

        }
    }
}

