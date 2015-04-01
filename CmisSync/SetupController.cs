//-----------------------------------------------------------------------
// <copyright file="SetupController.cs" company="GRAU DATA AG">
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

namespace CmisSync
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading;

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Cmis.UiUtils;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Storage.FileSystem;

    using log4net;

    /// <summary>
    /// Kind of pages that are used in the folder addition wizards.
    /// </summary>
    public enum PageType
    {
        None,
        Setup,
        Add1,
        Add2,
        Customize,
        Finished,
        Tutorial // This particular one contains sub-steps that are tracked via a number.
    }

    /// <summary>
    /// MVC controller for the two wizards:
    /// - CmisSync tutorial that appears at firt run,
    /// - wizard to add a new remote folder.
    /// </summary>
    public class SetupController
    {
        protected static readonly ILog Logger = LogManager.GetLogger(typeof(SetupController));

        // Delegates.
        public event Action ShowWindowEvent = delegate { };
        public event Action HideWindowEvent = delegate { };

        public event ChangePageEventHandler ChangePageEvent = delegate { };
        public delegate void ChangePageEventHandler(PageType page);

        public event UpdateProgressBarEventHandler UpdateProgressBarEvent = delegate { };
        public delegate void UpdateProgressBarEventHandler(double percentage);

        public event UpdateSetupContinueButtonEventHandler UpdateSetupContinueButtonEvent = delegate { };
        public delegate void UpdateSetupContinueButtonEventHandler(bool button_enabled);

        public event UpdateAddProjectButtonEventHandler UpdateAddProjectButtonEvent = delegate { };
        public delegate void UpdateAddProjectButtonEventHandler(bool button_enabled);

        public event ChangeAddressFieldEventHandler ChangeAddressFieldEvent = delegate { };
        public delegate void ChangeAddressFieldEventHandler(string text, string example_text);

        public event ChangeRepositoryFieldEventHandler ChangeRepositoryFieldEvent = delegate { };
        public delegate void ChangeRepositoryFieldEventHandler(string text, string example_text);

        public event ChangePathFieldEventHandler ChangePathFieldEvent = delegate { };
        public delegate void ChangePathFieldEventHandler(string text, string example_text);

        public event ChangeUserFieldEventHandler ChangeUserFieldEvent = delegate { };
        public delegate void ChangeUserFieldEventHandler(string text, string example_text);

        public event ChangePasswordFieldEventHandler ChangePasswordFieldEvent = delegate { };
        public delegate void ChangePasswordFieldEventHandler(string text, string example_text);

        public event LocalPathExistsEventHandler LocalPathExists;
        public delegate bool LocalPathExistsEventHandler(string path);

        /// <summary>
        /// Whether the window is currently open.
        /// </summary>
        public bool WindowIsOpen { get; private set; }

        /// <summary>
        /// Current step of the tutorial.
        /// </summary>
        public int TutorialCurrentPage { get; private set; }

        /// <summary>
        /// Current step of the remote folder addition wizard.
        /// </summary>
        private PageType FolderAdditionWizardCurrentPage;

        public Uri PreviousAddress { get; private set; }
        public string PreviousPath { get; private set; }
        public string PreviousRepository { get; private set; }
        public string SyncingReponame { get; private set; }
        public string DefaultRepoPath { get; private set; }
        public double ProgressBarPercentage { get; private set; }

        public Uri saved_address = null;
        public string saved_binding = CmisRepoCredentials.BindingBrowser;
        public string saved_remote_path = String.Empty;
        public string saved_user = String.Empty;
        public string saved_password = String.Empty;
        public string saved_repository = String.Empty;
        public string saved_local_path = String.Empty;
        public List<string> ignoredPaths = new List<string>();

        /// <summary>
        /// List of the CMIS repositories at the chosen URL.
        /// </summary>
        public IList<LogonRepositoryInfo> repositories;

        /// <summary>
        /// Whether CmisSync should be started automatically at login.
        /// </summary>
        private bool create_startup_item = true;

        /// <summary>
        /// Load repositories information from a CMIS endpoint.
        /// </summary>
        public static LoginCredentials GetRepositories(ServerCredentials credentials) {
            var multipleCredentials = credentials.CreateFuzzyCredentials();
                foreach (var cred in multipleCredentials) {
                if (cred.LogIn()) {
                    return cred;
                }
            }

            multipleCredentials.Sort();
            return multipleCredentials[0];
        }

        /// <summary>
        /// Regex to check an HTTP/HTTPS URL.
        /// </summary>
        private Regex UrlRegex = new Regex(@"^" +
                    "(https?)://" +                                                 // protocol
                    "(([a-z\\d$_\\.\\+!\\*'\\(\\),;\\?&=-]|%[\\da-f]{2})+" +        // username
                    "(:([a-z\\d$_\\.\\+!\\*'\\(\\),;\\?&=-]|%[\\da-f]{2})+)?" +     // password
                    "@)?(?#" +                                                      // auth delimiter
                    ")((([a-z\\d]\\.|[a-z\\d][a-z\\d-]*[a-z\\d]\\.)*" +             // domain segments AND
                    "[a-z][a-z\\d-]*[a-z\\d]" +                                     // top level domain OR
                    "|((\\d|\\d\\d|1\\d{2}|2[0-4]\\d|25[0-5])\\.){3}" +             // IP address
                    "(\\d|[1-9]\\d|1\\d{2}|2[0-4]\\d|25[0-5])" +                    //
                    ")(:\\d+)?" +                                                   // port
                    ")((/+([a-z\\d$_\\.\\+!\\*'\\(\\),;:@&=-]|%[\\da-f]{2})*)*?)" + // path
                    "$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Regex to check a CmisSync repository local folder name.
        /// Basically, it should be a valid local filesystem folder name.
        /// </summary>
        private Regex RepositoryRegex = new Regex(@"^([a-zA-Z0-9][^*/><?\|:;]*)$");
        private Regex RepositoryRegexLinux = new Regex(@"^([a-zA-Z0-9][^*\\><?\|:;]*)$");

        /// <summary>
        /// Constructor.
        /// </summary>
        public SetupController() {
            Logger.Debug("Entering constructor.");

            this.TutorialCurrentPage = 0;
            this.PreviousAddress = null;
            this.PreviousPath = string.Empty;
            this.SyncingReponame = string.Empty;
            this.DefaultRepoPath = Program.Controller.FoldersPath;

            // Actions.

            this.ChangePageEvent += delegate(PageType page) {
                this.FolderAdditionWizardCurrentPage = page;
            };

            Program.Controller.ShowSetupWindowEvent += delegate(PageType page) {
                if (this.FolderAdditionWizardCurrentPage == PageType.Finished) {
                    this.ShowWindowEvent();
                    return;
                }

                if (page == PageType.Add1) {
                    if (this.WindowIsOpen) {
                        if (this.FolderAdditionWizardCurrentPage == PageType.Finished ||
                            this.FolderAdditionWizardCurrentPage == PageType.None) {
                            this.ChangePageEvent(PageType.Add1);
                        }

                        this.ShowWindowEvent();

                    } else if (this.TutorialCurrentPage == 0) {
                        this.WindowIsOpen = true;
                        this.ChangePageEvent(PageType.Add1);
                        this.ShowWindowEvent();
                    }

                    return;
                }

                this.WindowIsOpen = true;
                this.ChangePageEvent(page);
                this.ShowWindowEvent();
            };
            Logger.Debug("Exiting constructor.");
        }

        /// <summary>
        /// User pressed the "Cancel" button, hide window.
        /// </summary>
        public void PageCancelled() {
            this.PreviousAddress = null;
            this.PreviousRepository = string.Empty;
            this.PreviousPath = string.Empty;
            this.ignoredPaths.Clear();
            this.TutorialCurrentPage = 0;

            this.WindowIsOpen = false;
            this.HideWindowEvent();
        }

        public void CheckSetupPage() {
            this.UpdateSetupContinueButtonEvent(true);
        }

        /// <summary>
        /// First-time wizard has been cancelled, so quit DataSpace Sync.
        /// </summary>
        public void SetupPageCancelled() {
            Program.Controller.Quit();
        }

        /// <summary>
        /// Move to second page of the tutorial.
        /// </summary>
        public void SetupPageCompleted() {
            this.TutorialCurrentPage = 1;
            this.ChangePageEvent(PageType.Tutorial);
        }

        /// <summary>
        /// Tutorial has been skipped, go to last step of wizard.
        /// </summary>
        public void TutorialSkipped() {
            this.TutorialCurrentPage = 4;
            this.ChangePageEvent(PageType.Tutorial);
        }

        /// <summary>
        /// Go to next step of the tutorial.
        /// </summary>
        public void TutorialPageCompleted() {
            this.TutorialCurrentPage++;

            // If last page reached, close tutorial.
            if (this.TutorialCurrentPage == 5) {
                this.TutorialCurrentPage = 0;
                this.FolderAdditionWizardCurrentPage = PageType.None;

                this.WindowIsOpen = false;
                this.HideWindowEvent();

                // If requested, add CmisSync to the list of programs to be started up when the user logs into Windows.
                if (this.create_startup_item) {
                    new Thread(() => Program.Controller.CreateStartupItem()).Start();
                }
            } else {
                // Go to next step of tutorial.
                this.ChangePageEvent(PageType.Tutorial);
            }
        }

        /// <summary>
        /// Checkbox to add CmisSync to the list of programs to be started up when the user logs into.
        /// </summary>
        public void StartupItemChanged(bool create_startup_item) {
            this.create_startup_item = create_startup_item;
        }

        /// <summary>
        /// Check whether the address is syntaxically valid.
        /// If OK, enable button to next step.
        /// </summary>
        /// <param name="address">URL to check</param>
        /// <returns>validity error, or empty string if valid</returns>
        public string CheckAddPage(string address) {
            address = address.Trim();

            // Check address validity.
            bool fields_valid = !string.IsNullOrEmpty(address) && this.UrlRegex.IsMatch(address);
            if (fields_valid) {
                this.saved_address = new Uri(address);
            }

            // Enable button to next step.
            this.UpdateAddProjectButtonEvent(fields_valid);

            // Return validity error, or empty string if valid.
            if (string.IsNullOrEmpty(address)) {
                return "EmptyURLNotAllowed";
            }

            if (!this.UrlRegex.IsMatch(address)) {
                return "InvalidURL";
            }

            return string.Empty;
        }

        /// <summary>
        /// Check local repository path and repo name.
        /// </summary>
        /// <param name="localpath"></param>
        /// <param name="reponame"></param>
        /// <returns>validity error, or empty string if valid</returns>
        public string CheckRepoPathAndName(string localpath, string reponame) {
            try {
                // Check whether foldername is already in use
                int index = Program.Controller.Folders.FindIndex(x => x.Equals(reponame, StringComparison.OrdinalIgnoreCase));
                if (index != -1) {
                    throw new ArgumentException(string.Format(Properties_Resources.FolderAlreadyExist, reponame));
                }

                // Check whether folder name contains invalid characters.
                Regex regexRepoName = Path.DirectorySeparatorChar.Equals('\\') ? this.RepositoryRegex : this.RepositoryRegexLinux;
                if (!regexRepoName.IsMatch(reponame) || CmisSync.Lib.Utils.IsInvalidFolderName(reponame.Replace(Path.DirectorySeparatorChar, ' '), new List<string>())) {
                    throw new ArgumentException(string.Format(Properties_Resources.InvalidRepoName, reponame));
                }

                // Validate localpath
                localpath = localpath.TrimEnd(Path.DirectorySeparatorChar);
                if (CmisSync.Lib.Utils.IsInvalidFolderName(Path.GetFileName(localpath), new List<string>())) {
                    throw new ArgumentException(string.Format(Properties_Resources.InvalidFolderName, Path.GetFileName(localpath)));
                }

                IDirectoryInfo dir = new CmisSync.Lib.Storage.FileSystem.DirectoryInfoWrapper(new DirectoryInfo(localpath));
                while (!dir.Exists) {
                    dir = dir.Parent;
                }

                if (!dir.IsExtendedAttributeAvailable()) {
                    throw new ArgumentException(string.Format("The filesystem where {0} points to is not able to save extended attributes, please choose another drive or turn on extended attributes", localpath));
                }

                // If no warning handler is registered, handle warning as error
                if (this.LocalPathExists == null) {
                    this.CheckRepoPathExists(localpath);
                }

                this.UpdateAddProjectButtonEvent(true);
                return string.Empty;
            } catch (Exception e) {
                this.UpdateAddProjectButtonEvent(false);
                return e.Message;
            }
        }

        public void CheckRepoPathExists(string localpath) {
            if (Directory.Exists(localpath)) {
                throw new ArgumentException(string.Format(Properties_Resources.LocalDirectoryExist));
            }
        }

        /// <summary>
        /// First step of remote folder addition wizard is complete, switch to second step
        /// </summary>
        public void Add1PageCompleted(Uri address, string binding, string user, string password) {
            this.saved_address = address;
            this.saved_binding = binding;
            this.saved_user = user;
            this.saved_password = password;

            this.ChangePageEvent(PageType.Add2);
        }

        /// <summary>
        /// Switch back from second to first step, presumably to change server or user.
        /// </summary>
        public void BackToPage1() {
            this.PreviousAddress = this.saved_address;
            this.ChangePageEvent(PageType.Add1);
        }

        /// <summary>
        /// Second step of remote folder addition wizard is complete, switch to customization step.
        /// </summary>
        public void Add2PageCompleted(string repository, string remote_path, string[] ignoredPaths, string[] selectedFolder) {
            this.SyncingReponame = Path.GetFileName(remote_path);
            this.ProgressBarPercentage = 1.0;

            this.ChangePageEvent(PageType.Customize);

            Uri address = this.saved_address;
            repository = repository.Trim();
            remote_path = remote_path.Trim();

            this.PreviousAddress = address;
            this.PreviousRepository = repository;
            this.PreviousPath = remote_path;

            this.ignoredPaths.Clear();
            foreach (string ignore in ignoredPaths) {
                this.ignoredPaths.Add(ignore);
            }
        }

        /// <summary>
        /// Second step of remote folder addition wizard is complete, switch to customization step.
        /// </summary>
        public void Add2PageCompleted(string repository, string remote_path) {
            this.Add2PageCompleted(repository, remote_path, new string[] { }, new string[] { });
        }

        /// <summary>
        /// Customization step of remote folder addition wizard is complete, start CmisSync.
        /// </summary>
        public void CustomizePageCompleted(string repoName, string localrepopath) {
            try {
                this.CheckRepoPathExists(localrepopath);
            } catch (ArgumentException) {
                if (this.LocalPathExists != null && !this.LocalPathExists(localrepopath)) {
                    return;
                }
            }

            this.SyncingReponame = repoName;
            this.saved_local_path = localrepopath;

            RepoInfo repoInfo = new RepoInfo {
                DisplayName = repoName,
                Address = this.saved_address,
                Binding = this.saved_binding,
                User = this.saved_user,
                ObfuscatedPassword = new Password(this.saved_password).ObfuscatedPassword,
                RepositoryId = this.PreviousRepository,
                RemotePath = this.PreviousPath,
                LocalPath = localrepopath
            };

            foreach (string ignore in this.ignoredPaths) {
                repoInfo.AddIgnorePath(ignore);
            }

            // Check that the folder exists.
            if (Directory.Exists(repoInfo.LocalPath)) {
                Logger.Info(string.Format("DataSpace Repository Folder {0} already exist, this could lead to sync conflicts", repoInfo.LocalPath));
            } else {
                // Create the local folder.
                Directory.CreateDirectory(repoInfo.LocalPath);
            }

            try {
                new Thread(() => {
                    Program.Controller.AddRepo(repoInfo);
                }).Start();
            } catch (Exception ex) {
                Logger.Fatal(ex.ToString());
            }

            this.ChangePageEvent(PageType.Finished);
        }

        /// <summary>
        /// Switch back from customization to step 2 of the remote folder addition wizard.
        /// </summary>
        public void BackToPage2() {
            this.ignoredPaths.Clear();
            this.ChangePageEvent(PageType.Add2);
        }

        /// <summary>
        /// Remote folder has been added, switch to the final step of the wizard.
        /// </summary>
        private void AddPageFetchedDelegate(string remote_url) {
            this.ChangePageEvent(PageType.Finished);

            Program.Controller.FolderFetched -= this.AddPageFetchedDelegate;
        }

        /// <summary>
        /// User clicked on the button to open the newly-created synchronized folder in the local file explorer.
        /// </summary>
        public void OpenFolderClicked() {
            Program.Controller.OpenCmisSyncFolder(this.SyncingReponame);
            this.SyncingReponame = string.Empty;
            this.FinishPageCompleted();
        }

        /// <summary>
        /// Folder addition wizard is over, reset it for next use.
        /// </summary>
        public void FinishPageCompleted() {
            this.PreviousAddress = null;
            this.PreviousPath = string.Empty;

            this.FolderAdditionWizardCurrentPage = PageType.None;
            this.HideWindowEvent();
        }

        /// <summary>
        /// Gets the connections problem warning message internationalized.
        /// </summary>
        /// <returns>The connections problem warning.</returns>
        /// <param name="server">Tried server.</param>
        /// <param name="e">Returned Exception</param>
        public string GetConnectionsProblemWarning(LoginException e) {
            switch (e.Type) {
            case LoginExceptionType.PermissionDenied:
                return Properties_Resources.LoginFailedForbidden;
            case LoginExceptionType.ServerNotFound:
                return Properties_Resources.ConnectFailure;
            case LoginExceptionType.HttpsSendFailure:
                return Properties_Resources.SendFailureHttps;
            case LoginExceptionType.NameResolutionFailure:
                return Properties_Resources.NameResolutionFailure;
            case LoginExceptionType.HttpsTrustFailure:
                return Properties_Resources.TrustFailure;
            case LoginExceptionType.Unauthorized:
                return Properties_Resources.LoginFailedForbidden;
            default:
                return string.Format(
                    "{0}{1}{2}",
                    e.Message,
                    Environment.NewLine,
                    string.Format(Properties_Resources.Sorry, Properties_Resources.ApplicationName));
            }
        }
    }
}