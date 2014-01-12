using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

using DotCMIS.Client;

using CmisSync.Lib.Events;

namespace CmisSync.Lib.Sync.Strategy
{
    /// <summary>
    /// Crawler Strategy which crawls local and remote directories for finding differences between them.
    /// </summary>
    public class Crawler : ReportingSyncEventHandler
    {
        /// <summary>
        /// The Crawler Strategy is the last strategy in the event queue, so the priority is zero.
        /// </summary>
        public static readonly int CRAWLER_PRIORITY = 0;
        private IFolder RemoteFolder;
        private DirectoryInfo LocalFolder;
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Sync.Strategy.Crawler"/> class.
        /// </summary>
        /// <param name='queue'>
        /// Queue to report events to.
        /// </param>
        /// <param name='remoteFolder'>
        /// Remote folder, which is the root of the crawl strategy.
        /// </param>
        /// <param name='localFolder'>
        /// Local folder, which is the root of the crawl strategy.
        /// </param>
        public Crawler (SyncEventQueue queue, IFolder remoteFolder, DirectoryInfo localFolder) : base(queue)
        {
            if(localFolder == null)
                throw new ArgumentNullException("Given local folder is null");
            if(remoteFolder == null)
                throw new ArgumentNullException("Given remote folder is null");
            this.RemoteFolder = remoteFolder;
            this.LocalFolder = localFolder;
        }

        /// <summary>
        ///  May not be changed during runtime 
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        public override int Priority {
            get {
                return CRAWLER_PRIORITY;
            }
        }

        /// <summary>
        /// Handles the specified e.
        /// </summary>
        /// <param name='e'>
        /// If set to <c>true</c> e.
        /// </param>
        public override bool Handle (ISyncEvent e)
        {
            if(e is CrawlRequestEvent)
            {
                var request = e as CrawlRequestEvent;
                CrawlSync(request.RemoteFolder, request.LocalFolder);
                //StartAsync(request.RemoteFolder, request.LocalFolder);
                return true;
            }
            if(e is StartNextSyncEvent)
            {
                CrawlSync(RemoteFolder, LocalFolder);
                //StartAsync(RemoteFolder, LocalFolder);
                Queue.AddEvent(new FullSyncCompletedEvent(e as StartNextSyncEvent));
                return true;
            }
            return false;
        }

        private void StartAsync(IFolder remoteFolder, DirectoryInfo localFolder) {
            using (var task = new Task(() => CrawlSync(remoteFolder, localFolder))) {
                task.Start();
            }
        }

        /// <summary>
        /// Synchronize by checking all folders/files one-by-one.
        /// This strategy is used if the CMIS server does not support the ChangeLog feature or as fallback if other methods failed.
        /// </summary>
        private void CrawlSync (IFolder remoteFolder, DirectoryInfo localFolder)
        {
            // Sets of local files/folders.
            ISet<string> localFileNames = new HashSet<string> ();
            ISet<string> localDirNames = new HashSet<string> ();

            // Collect all local folder and file names existing in local folder
            foreach(DirectoryInfo subdir in localFolder.GetDirectories())
            {
                localDirNames.Add(subdir.Name);
            }
            foreach(FileInfo file in localFolder.GetFiles())
            {
                localFileNames.Add(file.Name);
            }

            // Collect all child objects of the remote folder and figure out differences
            IItemEnumerable<ICmisObject> remoteChildren = remoteFolder.GetChildren();
            foreach (ICmisObject cmisObject in remoteChildren) {
                if (cmisObject is IFolder) {
                    IFolder folder = cmisObject as IFolder;
                    if(localDirNames.Contains(folder.Name)) {
                        // Both sides do have got the same folder name
                        // Synchronize metadata if different
                        Queue.AddEvent(new FolderEvent(
                            localFolder : new DirectoryInfo(Path.Combine(localFolder.FullName, folder.Name)),
                            remoteFolder: folder){Recursive = false});
                        // Recursive crawl the content of the folder
                        Queue.AddEvent(new CrawlRequestEvent(
                            localFolder : new DirectoryInfo(Path.Combine(localFolder.FullName, folder.Name)),
                            remoteFolder: folder));
                        // Remove handled folder from set to get only the local only folders back from set if done
                        localDirNames.Remove(folder.Name);
                    } else {
                        // Remote folder detected, which is not available locally
                        // Figure out, what to do with it
                        Queue.AddEvent(new FolderEvent(
                            localFolder : localFolder,
                            remoteFolder: folder) {
                            Recursive = true,
                            Remote = MetaDataChangeType.CREATED});
                    }
                }else if(cmisObject is IDocument) {
                    IDocument doc = cmisObject as IDocument;
                    if(localFileNames.Contains(doc.Name)) {
                        // Both sides do have got the file, synchronize them if different
                        Queue.AddEvent( new FileEvent(
                            localFile : new FileInfo(Path.Combine(localFolder.FullName, doc.Name)),
                            localParentDirectory : localFolder,
                            remoteFile : doc));
                        // Remove handled file from set
                        localFileNames.Remove(doc.Name);
                    } else {
                        // Only remote has got a file, figure out what to do
                        Queue.AddEvent(new FileEvent(
                            localFile : new FileInfo(Path.Combine(localFolder.FullName, doc.Name)),
                            localParentDirectory : localFolder,
                            remoteFile: doc){Remote = MetaDataChangeType.CREATED});
                    }
                }
            }
            // Only local folders are available, inform synchronizer about them
            foreach(string folder in localDirNames) {
                Queue.AddEvent(new FolderEvent(
                    localFolder : new DirectoryInfo(Path.Combine(localFolder.FullName, folder)),
                    remoteFolder: remoteFolder));
            }
            // Only local files are available, inform synchronizer about them
            foreach(string file in localFileNames) {
                Queue.AddEvent(new FileEvent(
                    localFile : new FileInfo(Path.Combine(localFolder.FullName, file)),
                    localParentDirectory : localFolder));
            }
        }
    }
}

