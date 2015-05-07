//-----------------------------------------------------------------------
// <copyright file="ControllerBase.cs" company="GRAU DATA AG">
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

namespace CmisSync {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading;

    using CmisSync.Lib;
    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Filter;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.FileSystem;

    using log4net;

    #if __COCOA__
    using Edit = CmisSync.EditWizardController;
    #endif

    /// <summary>
    /// Platform-independant part of the main CmisSync controller.
    /// </summary>
    public abstract class ControllerBase : IActivityListener, IDisposable {
        public readonly string BrandConfigFolder = "ClientBrand";

        /// <summary>
        /// Log4Net logger.
        /// </summary>
        protected static readonly ILog Logger = LogManager.GetLogger(typeof(ControllerBase));

        /// <summary>
        /// Whether it is the first time that CmisSync is being run.
        /// </summary>
        private bool firstRun;

        /// <summary>
        /// Keeps track of whether a download or upload is going on, for display of the task bar animation.
        /// </summary>
        private ActivityListenerAggregator activityListenerAggregator;

        private TransmissionManager transmissionManager;

        /// <summary>
        /// Concurrency locks.
        /// </summary>
        private object repoLock = new object();

        private object brandLock = new object();
        private bool firstCheckBrand = true;

        /// <summary>
        /// List of the CmisSync synchronized folders.
        /// </summary>
        private List<Repository> repositories = new List<Repository>();

        /// <summary>
        /// Dictionary of the edit folder diaglogs
        /// Key: synchronized folder name
        /// Value: <c>Edit</c>
        /// </summary>
        private Dictionary<string, Edit> edits = new Dictionary<string, Edit>();

        /// <summary>
        /// All the info about the CmisSync synchronized folder being created.
        /// </summary>
        private RepoInfo repoInfo;

        /// <summary>
        /// Is this controller disposed already?
        /// </summary>
        private bool disposed = false;

        private RepositoryStatusAggregator statusAggregator = new RepositoryStatusAggregator();

        /// <summary>
        /// Gets a value indicating whether the reporsitories have finished loading.
        /// </summary>
        public bool RepositoriesLoaded { get; private set; }

        /// <summary>
        /// Gets path where the DataSpace Sync synchronized folders are by default.
        /// </summary>
        public string FoldersPath { get; private set; }

        public event ShowSetupWindowEventHandler ShowSetupWindowEvent = delegate { };

        public delegate void ShowSetupWindowEventHandler(PageType page_type);

        public event Action ShowSettingWindowEvent = delegate { };

        public event Action ShowTransmissionWindowEvent = delegate { };

        public event Action ShowAboutWindowEvent = delegate { };

        public event FolderFetchedEventHandler FolderFetched = delegate { };

        public delegate void FolderFetchedEventHandler(string remote_url);

        public event FolderFetchingHandler FolderFetching = delegate { };

        public delegate void FolderFetchingHandler(double percentage);

        public event Action FolderListChanged = delegate { };

        public event Action OnTransmissionListChanged = delegate { };

        public event Action OnIdle = delegate { };

        public event Action OnSyncing = delegate { };

        public event Action OnError = delegate { };

        public event AlertNotificationRaisedEventHandler AlertNotificationRaised = delegate { };

        public delegate void AlertNotificationRaisedEventHandler(string title, string message);

        public delegate void ShowChangePasswordEventHandler(string reponame);

        public event ShowChangePasswordEventHandler ShowChangePassword = delegate { };

        public delegate void ShowExceptionExceptionEventHandler(string title, string msg);

        public event ShowExceptionExceptionEventHandler ShowException = delegate { };

        public delegate void SuccessfulLoginEventHandler(string reponame);

        public event SuccessfulLoginEventHandler SuccessfulLogin = delegate { };

        public delegate void ProxyAuthRequiredEventHandler(string reponame);

        public event ProxyAuthRequiredEventHandler ProxyAuthReqired = delegate { };

        /// <summary>
        /// Constructor for the general controller.
        /// </summary>
        public ControllerBase() {
            this.FoldersPath = ConfigManager.CurrentConfig.GetFoldersPath();
            this.transmissionManager = new TransmissionManager();
            this.activityListenerAggregator = new ActivityListenerAggregator(this, this.transmissionManager);
            this.transmissionManager.ActiveTransmissions.CollectionChanged += delegate(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
                this.OnTransmissionListChanged();
            };
            this.FolderListChanged += delegate {
                new Thread(() => {
                    lock (brandLock) {
                        if (CheckBrand(firstCheckBrand)) {
                            firstCheckBrand = false;
                            return;
                        }

                        SetupBrand();
                    }
                }).Start();
            };

            this.statusAggregator.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => {
                switch (this.statusAggregator.Status) {
                case SyncStatus.Idle:
                    this.OnIdle();
                    break;
                case SyncStatus.Synchronizing:
                    this.OnSyncing();
                    break;
                case SyncStatus.Warning:
                    this.OnError();
                    break;
                default:
                    this.OnIdle();
                    break;
                }
            };
        }

        /// <summary>
        /// Gets the repositories configured in CmisSync.
        /// </summary>
        public Repository[] Repositories {
            get {
                lock (this.repoLock) {
                    return this.repositories.GetRange(0, this.repositories.Count).ToArray();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether it is the first time that DataSpace Sync is being run.
        /// </summary>
        public bool FirstRun {
            get {
                return this.firstRun;
            }
        }

        /// <summary>
        /// Gets the list of synchronized folders.
        /// </summary>
        public List<string> Folders {
            get {
                List<string> folders = new List<string>();
                foreach (RepoInfo f in ConfigManager.CurrentConfig.Folders) {
                    folders.Add(f.DisplayName);
                }

                folders.Sort();
                return folders;
            }
        }

        /// <summary>
        /// Gets a value indicating whether any edit window is visible.
        /// </summary>
        /// <value><c>true</c> if this instance is edit window visible; otherwise, <c>false</c>.</value>
        public bool IsEditWindowVisible {
            get {
                lock (this.repoLock) {
                    return this.edits.Count > 0;
                }
            }

            private set {
            }
        }

        /// <summary>
        /// Add CmisSync to the list of programs to be started up when the user logs into Windows.
        /// </summary>
        public abstract void CreateStartupItem();

        /// <summary>
        /// Add CmisSync to the user's Windows Explorer bookmarks.
        /// </summary>
        public abstract void AddToBookmarks();

        /// <summary>
        /// Creates the CmisSync folder in the user's home folder.
        /// </summary>
        /// <returns><c>true</c> if the folder was created, <c>false</c> if it exists already</returns>
        public abstract bool CreateCmisSyncFolder();

        /// <summary>
        /// List of actives transmissions.
        /// </summary>
        /// <returns>The transmissions.</returns>
        public List<Transmission> ActiveTransmissions() {
            return this.transmissionManager.ActiveTransmissionsAsList();
        }

        /// <summary>
        /// Initialize the controller.
        /// </summary>
        /// <param name="firstRun">Whether it is the first time that DataSpace Sync is being run.</param>
        public virtual void Initialize(bool firstRun) {
            this.firstRun = firstRun;

            // Create the CmisSync folder and add it to the bookmarks
            if (this.CreateCmisSyncFolder()) {
                this.AddToBookmarks();
            }
        }

        /// <summary>
        /// Once the UI has loaded, show setup window if it is the first run, or check the repositories.
        /// </summary>
        public void UIHasLoaded() {
            if (this.firstRun) {
                this.ShowSetupWindow(PageType.Setup);
            } else {
                new Thread(() => {
                    CheckRepositories();
                    RepositoriesLoaded = true;

                    //// Update UI.
                    // FolderListChanged();
                }).Start();
            }
        }

        /// <summary>
        /// Removes the repository from synchronisation.
        /// </summary>
        /// <param name="reponame">Reponame of the repository, which should be removed from sync.</param>
        public void RemoveRepositoryFromSync(string reponame) {
            lock (this.repoLock) {
                RepoInfo f = ConfigManager.CurrentConfig.GetRepoInfo(reponame);
                if (f != null) {
                    Edit edit = null;
                    if (this.edits.TryGetValue(reponame, out edit)) {
                        edit.Controller.CloseWindow();
                    }

                    this.RemoveRepository(f);
                    ConfigManager.CurrentConfig.Folders.Remove(f);
                    ConfigManager.CurrentConfig.Save();
                } else {
                    Logger.Warn("Reponame \"" + reponame + "\" could not be found: Removing Repository failed");
                }
            }

            // Update UI.
            this.FolderListChanged();
        }

        /// <summary>
        /// Edits the repository folder.
        /// </summary>
        /// <param name="reponame">Reponame of the repository.</param>
        public void EditRepositoryFolder(string reponame) {
            this.EditRepository(reponame, Edit.EditType.EditFolder);
        }

        /// <summary>
        /// Edits the repository credentials.
        /// </summary>
        /// <param name="reponame">Reponame of the repository.</param>
        public void EditRepositoryCredentials(string reponame) {
            this.EditRepository(reponame, Edit.EditType.EditCredentials);
        }

        /// <summary>
        /// Pause or un-pause synchronization for a particular folder.
        /// </summary>
        /// <param name="repoName">the folder to pause/unpause</param>
        public void StartOrSuspendRepository(string repoName) {
            lock (this.repoLock) {
                foreach (Repository repo in this.repositories) {
                    if (repo.Name == repoName) {
                        if (repo.Status != SyncStatus.Suspend) {
                            repo.Suspend();
                            Logger.Debug("Requested to suspend sync of repo " + repo.Name);
                        } else {
                            repo.Resume();
                            Logger.Debug("Requested to resume sync of repo " + repo.Name);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Stops/Suspends all active repositories.
        /// </summary>
        public void StopAll() {
            lock (this.repoLock) {
                foreach (var repo in this.Repositories) {
                    if (repo.Status != SyncStatus.Suspend) {
                        repo.Suspend();
                    }
                }

                Logger.Debug("Start to stop all active file transmissions");
                int wait = 0;
                do {
                    List<Transmission> activeList = this.transmissionManager.ActiveTransmissionsAsList();
                    foreach (var transmission in activeList) {
                        transmission.Abort();
                    }

                    if (activeList.Count > 0) {
                        Thread.Sleep(100);
                        wait++;
                    } else {
                        break;
                    }
                } while (wait < 100);
                Logger.Debug("Start to abort all open HttpWebRequests");
                this.transmissionManager.AbortAllRequests();
                Logger.Debug("Finish to stop all active file transmissions");
            }
        }

        /// <summary>
        /// Starts all stopped/suspended repositories.
        /// </summary>
        public void StartAll() {
            lock (this.repoLock) {
                foreach (var repo in this.repositories) {
                    repo.Resume();
                }
            }
        }

        /// <summary>
        /// Invokes the controller to create a new repository instance based on the given informations.
        /// </summary>
        /// <param name="info">Repository informations.</param>
        public void AddRepo(RepoInfo info) {
            lock (this.repoLock) {
                // Add folder to XML config file.
                ConfigManager.CurrentConfig.Folders.Add(info);
                ConfigManager.CurrentConfig.Save();

                // Initialize in the UI.
                this.AddRepository(info);
            }

            // Update UI.
            this.FolderListChanged();
        }

        /// <summary>
        /// Show first-time wizard.
        /// </summary>
        /// <param name="page_type">Page to show.</param>
        public void ShowSetupWindow(PageType page_type) {
            this.ShowSetupWindowEvent(page_type);
        }

        /// <summary>
        /// Show setting dialog
        /// </summary>
        public void ShowSettingWindow() {
            this.ShowSettingWindowEvent();
        }

        /// <summary>
        /// Show transmission window
        /// </summary>
        public void ShowTransmissionWindow() {
            this.ShowTransmissionWindowEvent();
        }

        /// <summary>
        /// Show info about DataSpace Sync
        /// </summary>
        public void ShowAboutWindow() {
            this.ShowAboutWindowEvent();
        }

        /// <summary>
        /// Quit DataSpace Sync Client.
        /// </summary>
        public virtual void Quit() {
            foreach (Repository repo in this.Repositories) {
                repo.Dispose();
            }

            Environment.Exit(0);
        }

        /// <summary>
        /// A download or upload has started, so run task icon animation.
        /// </summary>
        public void ActivityStarted() {
            this.OnSyncing();
        }

        /// <summary>
        /// No download nor upload, so no task icon animation.
        /// </summary>
        public void ActivityStopped() {
            this.OnIdle();
        }

        /// <summary>
        /// Releases all resource used by the <see cref="CmisSync.ControllerBase"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="CmisSync.ControllerBase"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="CmisSync.ControllerBase"/> in an unusable state. After
        /// calling <see cref="Dispose"/>, you must release all references to the <see cref="CmisSync.ControllerBase"/>
        /// so the garbage collector can reclaim the memory that the <see cref="CmisSync.ControllerBase"/> was occupying.</remarks>
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all repositories.
        /// </summary>
        /// <param name="disposing">If set to <c>true</c> disposing.</param>
        protected virtual void Dispose(bool disposing) {
            if (this.disposed) {
                return;
            }

            if (disposing) {
                lock(this.repoLock) {
                    foreach (var repo in this.repositories) {
                        repo.Dispose();
                    }
                }
            }

            this.disposed = true;
        }

        private bool CheckBrand(bool checkFiles) {
            Config config = ConfigManager.CurrentConfig;
            if (config.Brand == null || config.Brand.Server == null) {
                return false;
            }

            ClientBrand clientBrand = new ClientBrand();
            foreach (string path in clientBrand.PathList) {
                if (!File.Exists(Path.Combine(config.GetConfigPath(), this.BrandConfigFolder, path.Substring(1)))) {
                    return false;
                }
            }

            List<RepoInfo> folders;
            lock (this.repoLock) {
                folders = config.Folders.ToList();
            }

            foreach (RepoInfo folder in folders) {
                if (folder.Address.ToString() != config.Brand.Server.ToString()) {
                    continue;
                }

                if (!checkFiles) {
                    return true;
                }

                if (clientBrand.SetupServer(folder.Credentials)) {
                    bool success = true;
                    foreach (string path in clientBrand.PathList) {
                        DateTime date;
                        if (!clientBrand.GetFileDateTime(path, out date)) {
                            success = false;
                            break;
                        }

                        BrandFile file = config.Brand.Files.Find((BrandFile current) => { return current.Path == path; });
                        if (file == null || file.Date != date) {
                            success = false;
                            break;
                        }
                    }

                    if (success) {
                        return true;
                    }
                }
            }

            return false;
        }

        private void SetupBrand() {
            Config config = ConfigManager.CurrentConfig;

            List<RepoInfo> folders;
            lock (this.repoLock) {
                folders = config.Folders.ToList();
            }

            foreach (RepoInfo folder in folders) {
                List<BrandFile> files = new List<BrandFile>();
                ClientBrand clientBrand = new ClientBrand();
                if (clientBrand.SetupServer(folder.Credentials)) {
                    bool success = true;
                    foreach (string path in clientBrand.PathList) {
                        DateTime date;
                        if (!clientBrand.GetFileDateTime(path, out date)) {
                            success = false;
                            break;
                        }

                        string pathname = Path.Combine(config.GetConfigPath(), this.BrandConfigFolder, path.Substring(1));
                        Directory.CreateDirectory(Path.GetDirectoryName(pathname));
                        try {
                            using (FileStream output = File.OpenWrite(pathname)) {
                                if (!clientBrand.GetFile(path, output)) {
                                    success = false;
                                    break;
                                }
                            }
                        } catch (Exception e) {
                            Logger.Error(string.Format("Fail to update the cilent brand file {0}: {1}", pathname, e));
                            success = false;
                            break;
                        }

                        BrandFile file = new BrandFile();
                        file.Date = date;
                        file.Path = path;
                        files.Add(file);
                    }

                    if (success) {
                        config.Brand = new Brand();
                        config.Brand.Server = folder.Address;
                        config.Brand.Files = files;
                        lock (this.repoLock) {
                            config.Save();
                        }

                        return;
                    }
                }
            }

            config.Brand = null;
            lock (this.repoLock) {
                config.Save();
            }
        }

        /// <summary>
        /// Initialize (in the UI and syncing mechanism) an existing synchronized folder.
        /// </summary>
        /// <param name="repositoryInfo">Repository informations</param>
        private void AddRepository(RepoInfo repositoryInfo) {
            try {
                Repository repo = new Repository(repositoryInfo, this.activityListenerAggregator);
                this.transmissionManager.AddPathRepoMapping(repositoryInfo.LocalPath, repositoryInfo.DisplayName);
                repo.ShowException += (object sender, RepositoryExceptionEventArgs e) => {
                    string msg = string.Empty;
                    switch (e.Type) {
                    case ExceptionType.LocalSyncTargetDeleted:
                        msg = string.Format(Properties_Resources.LocalRootFolderUnavailable, repositoryInfo.LocalPath);
                        break;
                    default:
                        msg = e.Exception != null ? e.Exception.Message : Properties_Resources.UnknownExceptionOccured;
                        break;
                    }

                    switch (e.Level) {
                    case ExceptionLevel.Fatal:
                        this.AlertNotificationRaised(string.Format(Properties_Resources.FatalExceptionTitle, repositoryInfo.DisplayName), msg);
                        break;
                    case ExceptionLevel.Warning:
                        this.ShowException(string.Format(Properties_Resources.WarningExceptionTitle, repositoryInfo.DisplayName), msg);
                        break;
                    default:
                        this.ShowException(string.Format(Properties_Resources.WarningExceptionTitle, repositoryInfo.DisplayName), msg);
                        break;
                    }
                };
                repo.Queue.EventManager.AddEventHandler(new GenericHandleDublicatedEventsFilter<PermissionDeniedEvent, SuccessfulLoginEvent>());
                repo.Queue.EventManager.AddEventHandler(new GenericHandleDublicatedEventsFilter<ProxyAuthRequiredEvent, SuccessfulLoginEvent>());
                repo.Queue.EventManager.AddEventHandler(
                    new GenericSyncEventHandler<ProxyAuthRequiredEvent>(
                    0,
                    delegate(ISyncEvent e) {
                    this.ProxyAuthReqired(repositoryInfo.DisplayName);
                    return true;
                }));
                repo.Queue.EventManager.AddEventHandler(
                    new GenericSyncEventHandler<PermissionDeniedEvent>(
                    0,
                    delegate(ISyncEvent e) {
                    var permissionDeniedEvent = e as PermissionDeniedEvent;
                    if (permissionDeniedEvent.IsBlockedUntil == null) {
                        this.ShowChangePassword(repositoryInfo.DisplayName);
                    } else {
                        this.ShowException(
                            string.Format(Properties_Resources.LoginFailed, repo.Name),
                            string.Format(Properties_Resources.LoginFailedLockedUntil, permissionDeniedEvent.IsBlockedUntil));
                    }

                    return true;
                }));
                repo.Queue.EventManager.AddEventHandler(
                    new GenericSyncEventHandler<SuccessfulLoginEvent>(
                    0,
                    delegate(ISyncEvent e) {
                    this.SuccessfulLogin(repositoryInfo.DisplayName);
                    return false;
                }));
                repo.Queue.EventManager.AddEventHandler(new GenericSyncEventHandler<ConfigurationNeededEvent>(
                    1,
                    delegate(ISyncEvent e) {
                    this.ShowException("The configuration of " + repo.Name + " is broken", "Please reconfigure the connection");
                    return true;
                }));
                repo.Queue.EventManager.AddEventHandler(new GenericSyncEventHandler<InteractionNeededEvent>(
                    1,
                    delegate(ISyncEvent e) {
                    var interactionEvent = e as InteractionNeededEvent;
                    this.ShowException(interactionEvent.Title, interactionEvent.Description);
                    return true;
                }));
                repo.Queue.EventManager.AddEventHandler(new GenericSyncEventHandler<ExceptionEvent>(
                    0,
                    delegate(ISyncEvent e) {
                    var ex = (e as ExceptionEvent).Exception;
                    this.ShowException("Exception on " + repo.Name, ex.Message);
                    return false;
                }));
                this.repositories.Add(repo);
                this.statusAggregator.Add(repo);
                repo.Initialize();
            } catch (ExtendedAttributeException extendedAttributeException) {
                this.ShowException(
                    string.Format(Properties_Resources.CannotSync, this.repoInfo.DisplayName),
                    string.Format(Properties_Resources.ProblemWithFS, Environment.NewLine, extendedAttributeException.Message));
            }
        }

        private void EditRepository(string reponame, Edit.EditType type) {
            RepoInfo folder;

            lock (this.repoLock) {
                folder = ConfigManager.CurrentConfig.GetRepoInfo(reponame);
                if (folder == null) {
                    Logger.Warn("Reponame \"" + reponame + "\" could not be found: Editing Repository failed");
                    return;
                }

                Edit edit = null;
                if (this.edits.TryGetValue(reponame, out edit)) {
                    edit.Controller.OpenWindow();
                    return;
                }

                CmisRepoCredentials credentials = new CmisRepoCredentials() {
                    Address = folder.Address,
                    Binding = folder.Binding,
                    UserName = folder.User,
                    Password = new Password() {
                        ObfuscatedPassword = folder.ObfuscatedPassword
                    },
                    RepoId = folder.RepositoryId
                };
                List<string> oldIgnores = new List<string>();
                foreach (var ignore in folder.IgnoredFolders) {
                    if (!string.IsNullOrEmpty(ignore.Path)) {
                        oldIgnores.Add(ignore.Path);
                    }
                }

                edit = new Edit(type, credentials, folder.DisplayName, folder.RemotePath, oldIgnores, folder.LocalPath);
                this.edits.Add(reponame, edit);

                edit.Controller.SaveFolderEvent += delegate {
                    lock (this.repoLock) {
                        folder.IgnoredFolders.Clear();
                        foreach (string ignore in edit.Ignores) {
                            folder.AddIgnorePath(ignore);
                        }

                        folder.SetPassword(edit.Credentials.Password);
                        ConfigManager.CurrentConfig.Save();
                        foreach (Repository repo in this.repositories) {
                            if (repo.Name == reponame) {
                                repo.Queue.AddEvent(new RepoConfigChangedEvent(folder));
                            }
                        }
                    }
                };

                edit.Controller.CleanWindowEvent += delegate {
                    lock (this.repoLock) {
                        this.edits.Remove(reponame);
                    }
                };
                edit.Controller.OpenWindow();
            }
        }

        /// <summary>
        /// Remove a synchronized folder from the CmisSync configuration.
        /// This happens after the user removes the folder.
        /// </summary>
        /// <param name="folder">The synchronized folder to remove</param>
        private void RemoveRepository(RepoInfo folder) {
            foreach (Repository repo in this.repositories) {
                if (repo.LocalPath.Equals(folder.LocalPath)) {
                    repo.Dispose();
                    this.repositories.Remove(repo);
                    this.statusAggregator.Remove(repo);
                    repo.Dispose();
                    break;
                }
            }

            // Remove DBreeze DB folder
            try {
                Directory.Delete(folder.GetDatabasePath(), true);
            } catch (DirectoryNotFoundException) {
            }
        }

        /// <summary>
        /// Check the configured CmisSync synchronized folders.
        /// Remove the ones whose folders have been deleted.
        /// </summary>
        private void CheckRepositories() {
            lock (this.repoLock) {
                List<RepoInfo> toBeDeleted = new List<RepoInfo>();

                // If folder has been deleted, remove it from configuration too.
                foreach (var f in ConfigManager.CurrentConfig.Folders) {
                    string folder_name = f.DisplayName;
                    string folder_path = f.LocalPath;

                    if (!Directory.Exists(folder_path)) {
                        this.RemoveRepository(f);
                        toBeDeleted.Add(f);

                        Logger.Info("Controller | Removed folder '" + folder_name + "' from config");
                    } else {
                        this.AddRepository(f);
                    }
                }

                foreach (var f in toBeDeleted) {
                    ConfigManager.CurrentConfig.Folders.Remove(f);
                }

                if (toBeDeleted.Count > 0) {
                    ConfigManager.CurrentConfig.Save();
                }
            }

            // Update UI.
            this.FolderListChanged();
        }

        /// <summary>
        /// Fix the file attributes of a folder, recursively.
        /// </summary>
        /// <param name="path">Folder to fix</param>
        private void ClearFolderAttributes(string path) {
            if (!Directory.Exists(path)) {
                return;
            }

            string[] folders = Directory.GetDirectories(path);

            foreach (string folder in folders) {
                this.ClearFolderAttributes(folder);
            }

            string[] files = Directory.GetFiles(path);

            foreach (string file in files) {
                if (!CmisSync.Lib.Utils.IsSymlink(file)) {
                    File.SetAttributes(file, FileAttributes.Normal);
                }
            }
        }
    }
}
