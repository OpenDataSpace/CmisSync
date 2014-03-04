//   CmisSync, a collaboration and sharing tool.
//   Copyright (C) 2010  Hylke Bons <hylkebons@gmail.com>
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General Public License for more details.
//
//   You should have received a copy of the GNU General Public License
//   along with this program. If not, see <http://www.gnu.org/licenses/>.


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using log4net;

using Timers = System.Timers;
using CmisSync.Lib.Events;
#if __COCOA__
using MonoMac.Foundation;
#endif
namespace CmisSync.Lib
{

    /// <summary>
    /// Synchronizes a remote folder.
    /// This class contains the loop that synchronizes every X seconds.
    /// </summary>
    public abstract class RepoBase : IDisposable
    {
        /// <summary>
        /// Log.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RepoBase));


        /// <summary>
        /// Perform a synchronization if one is not running already.
        /// </summary>
        public abstract void SyncInBackground();


        /// <summary>
        /// Local disk size taken by the repository.
        /// </summary>
        public abstract double Size { get; }


        /// <summary>
        /// Affect a new <c>SyncStatus</c> value.
        /// </summary>
        public Action<SyncStatus> SyncStatusChanged { get; set; }

        /// <summary>
        /// Path of the local synchronized folder.
        /// </summary>
        public readonly string LocalPath;


        /// <summary>
        /// Name of the synchronized folder, as found in the CmisSync XML configuration file.
        /// </summary>
        public readonly string Name;


        /// <summary>
        /// URL of the remote CMIS endpoint.
        /// </summary>
        public readonly Uri RemoteUrl;


        /// <summary>
        /// Current status of the synchronization (paused or not).
        /// </summary>
        public SyncStatus Status { get; private set; }


        /// <summary>
        /// Stop syncing momentarily.
        /// </summary>
        public void Suspend()
        {
            Status = SyncStatus.Suspend;
        }

        /// <summary>
        /// Restart syncing.
        /// </summary>
        public virtual void Resume()
        {
            Status = SyncStatus.Idle;
        }

        /// <summary>
        /// Gets the polling scheduler.
        /// </summary>
        /// <value>
        /// The scheduler.
        /// </value>
        public SyncScheduler Scheduler { get; private set; }

        /// <summary>
        /// Event Queue for this repository.
        /// Use this to notifiy events for this repository.
        /// </summary>
        public SyncEventQueue Queue { get; private set; }

        /// <summary>
        /// Event Manager for this repository.
        /// Use this for adding and removing SyncEventHandler for this repository.
        /// </summary>
        public SyncEventManager EventManager { get; private set; }

        /// <summary>
        /// Return the synchronized folder's information.
        /// </summary>
        protected RepoInfo RepoInfo { get; set; }


        /// <summary>
        /// Watches the local filesystem for changes.
        /// </summary>
        public CmisSync.Lib.Sync.Strategy.Watcher Watcher { get; private set; }

        /// <summary>
        /// The ignored folders filter.
        /// </summary>
        private Events.Filter.IgnoredFoldersFilter ignoredFoldersFilter;

        private Events.Filter.IgnoredFileNamesFilter ignoredFileNameFilter;

        /// <summary>
        /// Track whether <c>Dispose</c> has been called.
        /// </summary>
        private bool disposed = false;


        /// <summary>
        /// Constructor.
        /// </summary>
        public RepoBase(RepoInfo repoInfo)
        {
            EventManager = new SyncEventManager();
            EventManager.AddEventHandler(new DebugLoggingHandler());
            Queue = new SyncEventQueue(EventManager);
            RepoInfo = repoInfo;
            LocalPath = repoInfo.TargetDirectory;
            Name = repoInfo.Name;
            RemoteUrl = repoInfo.Address;
            ignoredFoldersFilter = new Events.Filter.IgnoredFoldersFilter(Queue){IgnoredPaths=new List<string>(repoInfo.getIgnoredPaths())};
            ignoredFileNameFilter = new CmisSync.Lib.Events.Filter.IgnoredFileNamesFilter(Queue){Wildcards = ConfigManager.CurrentConfig.IgnoreFileNames};
            EventManager.AddEventHandler(ignoredFileNameFilter);
            EventManager.AddEventHandler(ignoredFoldersFilter);
            EventManager.AddEventHandler(new GenericSyncEventHandler<RepoConfigChangedEvent>(0, RepoInfoChanged));
            // start scheduler
            Scheduler = new SyncScheduler(Queue, repoInfo.PollInterval);
            EventManager.AddEventHandler(Scheduler);

            EventManager.AddEventHandler(new GenericSyncEventHandler<StartNextSyncEvent>(0, delegate(ISyncEvent e) {
                SyncInBackground();
                return true;
            }));


            SyncStatusChanged += delegate(SyncStatus status)
            {
                Status = status;
            };
            #if __COCOA__
            this.Watcher = new CmisSync.Lib.Sync.Strategy.MacWatcher(LocalPath, Queue);
            #else
            this.Watcher = new CmisSync.Lib.Sync.Strategy.NetWatcher( new FileSystemWatcher(LocalPath), Queue);
            #endif
        }

        private bool RepoInfoChanged(ISyncEvent e)
        {
            if (e is RepoConfigChangedEvent)
            {
                this.RepoInfo = (e as RepoConfigChangedEvent).RepoInfo;
                this.ignoredFoldersFilter.IgnoredPaths = new List<string>(this.RepoInfo.getIgnoredPaths());
                this.ignoredFileNameFilter.Wildcards = ConfigManager.CurrentConfig.IgnoreFileNames;
                return true;
            }
            else
            {
                // This should never ever happen!
                return false;
            }
        }


        /// <summary>
        /// Destructor.
        /// </summary>
        ~RepoBase()
        {
            Dispose(false);
        }


        /// <summary>
        /// Implement IDisposable interface. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Dispose pattern implementation.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.Scheduler.Dispose();
                    this.Watcher.Dispose();
                    this.Queue.StopListener();
                    int timeout = 500;
                    if(!this.Queue.WaitForStopped(timeout))
                        Logger.Debug(String.Format("Event Queue is of {0} has not been closed in {1} miliseconds", RemoteUrl.ToString(), timeout));
                    this.Queue.Dispose();
                }
                this.disposed = true;
            }
        }


        /// <summary>
        /// Initialize the scheduled background sync processes.
        /// </summary>
        public virtual void Initialize()
        {
            // Sync up everything that changed
            // since we've been offline
            // start full crawl sync on beginning
            Queue.AddEvent(new StartNextSyncEvent(true));
            Scheduler.Start();
        }

        /// <summary>
        /// A conflict has been resolved.
        /// </summary>
        protected internal void OnConflictResolved()
        {
            // ConflictResolved(); TODO
        }


        /// <summary>
        /// Recursively gets a folder's size in bytes.
        /// </summary>
        private double CalculateSize(DirectoryInfo parent)
        {
            if (!Directory.Exists(parent.ToString()))
                return 0;

            double size = 0;

            try
            {
                // All files at this level.
                foreach (FileInfo file in parent.GetFiles())
                {
                    if (!file.Exists)
                        return 0;

                    size += file.Length;
                }

                // Recurse.
                foreach (DirectoryInfo directory in parent.GetDirectories())
                    size += CalculateSize(directory);

            }
            catch (Exception)
            {
                return 0;
            }

            return size;
        }
    }


    /// <summary>
    /// Current status of the synchronization.
    /// TODO: It was used in SparkleShare for up/down/error but is not useful anymore, should be removed.
    /// </summary>
    public enum SyncStatus
    {
        /// <summary>
        /// Normal operation.
        /// </summary>
        Idle,

        /// <summary>
        /// Synchronization is suspended.
        /// TODO this should be written in XML configuration instead.
        /// </summary>
        Suspend,

        /// <summary>
        /// Any sync conflict or warning happend
        /// </summary>
        Warning
    }
}
