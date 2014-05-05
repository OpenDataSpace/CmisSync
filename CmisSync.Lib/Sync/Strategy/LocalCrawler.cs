using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

using DotCMIS.Client;

using CmisSync.Lib.Events;
using CmisSync.Lib.Cmis;

namespace CmisSync.Lib
{
    public class LocalCrawler : AbstractEventProducer
    {

        private IDatabase Db;
        private DirectoryInfo LocalFolder;

        public LocalCrawler (IDatabase db, DirectoryInfo localFolder, SyncEventQueue queue) : base(queue)
        {
            if(db == null)
                throw new ArgumentNullException("Given Database is null");
            if(localFolder == null)
                throw new ArgumentNullException("Given local folder is null");
            this.Db = db;
            this.LocalFolder = localFolder;
        }

        public void start() {
            throw new NotImplementedException();
        }
    }
}

