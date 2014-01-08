using System;

using DotCMIS.Client;

namespace CmisSync.Lib.Events
{
    public class CrawlRequestEvent : ISyncEvent
    {
        public IFolder RemoteFolder { get; private set; }

        public string LocalFolder { get; private set; }

        public CrawlRequestEvent (string localFolder, IFolder remoteFolder)
        {
            if(String.IsNullOrEmpty(localFolder))
                throw new ArgumentNullException("Given path is null");
            if(remoteFolder == null)
                throw new ArgumentNullException("Given remote folder is null");
            RemoteFolder = remoteFolder;
            LocalFolder = localFolder;
        }
    }
}

