//-----------------------------------------------------------------------
// <copyright file="Edit.cs" company="GRAU DATA AG">
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
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using CmisSync.Lib.Config;
using CmisSync.Lib.Cmis.UiUtils;
using CmisSync.CmisTree;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CmisSync
{
    /// <summary>
    /// Edit folder diaglog
    /// It allows user to edit the selected and ignored folders
    /// </summary>
    class Edit : SetupWindow
    {
        /// <summary>
        /// Controller
        /// </summary>
        public EditController Controller = new EditController();


        /// <summary>
        /// Synchronized folder name
        /// </summary>
        public string FolderName;

        /// <summary>
        /// Ignore folder list
        /// </summary>
        public List<string> Ignores;

        /// <summary>
        /// Credentials
        /// </summary>
        public CmisRepoCredentials Credentials;

        private BackgroundWorker backgroundWorker = new BackgroundWorker();

        private string remotePath;
        private string localPath;

        public enum EditType {
            EditFolder,
            EditCredentials
        };

        private EditType type;

        /// <summary>
        /// Constructor
        /// </summary>
        public Edit(EditType type, CmisRepoCredentials credentials, string name, string remotePath, List<string> ignores, string localPath)
        {
            FolderName = name;
            this.Credentials = credentials;
            this.remotePath = remotePath;
            this.Ignores = new List<string>(ignores);
            this.localPath = localPath;
            this.type = type;
            this.backgroundWorker.WorkerSupportsCancellation = true;
            this.backgroundWorker.DoWork += CheckPassword;
            this.backgroundWorker.RunWorkerCompleted += PasswordChecked;

            CreateTreeView();
            LoadEdit();
            switch (type)
            {
                case EditType.EditFolder:
                    //  GUI workaround to remove ignore folder {{
                    //tab.SelectedItem = tabItemSelection;
                    //break;
                    //  GUI workaround to remove ignore folder }}
                case EditType.EditCredentials:
                    tab.SelectedItem = tabItemCredentials;
                    break;
                default:
                    break;
            }

            this.Title = Properties_Resources.EditTitle;
            this.Description = "";
            this.ShowAll();

            // Defines how to show the setup window.
            Controller.OpenWindowEvent += delegate
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    Show();
                    Activate();
                    BringIntoView();
                });
            };

            Controller.CloseWindowEvent += delegate
            {
                asyncLoader.Cancel();
            };

            finishButton.Click += delegate
            {
                Ignores = NodeModelUtils.GetIgnoredFolder(repo);
                Credentials.Password = passwordBox.Password;
                Controller.SaveFolder();
                Close();
            };

            cancelButton.Click += delegate
            {
                Close();
            };
        }


        protected override void Close(object sender, CancelEventArgs args)
        {
            backgroundWorker.CancelAsync();
            backgroundWorker.Dispose();
            Controller.CloseWindow();
        }


        CmisSync.CmisTree.RootFolder repo;
        private AsyncNodeLoader asyncLoader;

        TreeView treeView;
        private TabControl tab;
        private TabItem tabItemSelection;
        private TabItem tabItemCredentials;
        private TextBlock addressLabel;
        private TextBox addressBox;
        private TextBlock userLabel;
        private TextBox userBox;
        private TextBlock passwordLabel;
        private PasswordBox passwordBox;
        private CircularProgressBar passwordProgress;
        private TextBlock passwordHelp;
        bool passwordChanged;
        private Button finishButton;
        private Button cancelButton;


        private void CheckPassword(object sender, DoWorkEventArgs args)
        {
            if (!passwordChanged) {
                return;
            }

            Dispatcher.BeginInvoke((Action)delegate
            {
                passwordHelp.Text = Properties_Resources.LoginCheck;
                passwordBox.IsEnabled = false;
                passwordProgress.Visibility = Visibility.Visible;
            });

            ServerCredentials cred = new ServerCredentials()
            {
                Address = Credentials.Address,
                UserName = Credentials.UserName,
                Password = passwordBox.Password
            };

            CmisUtils.GetRepositories(cred);
        }

        private void PasswordChecked(object sender, RunWorkerCompletedEventArgs args)
        {
            if (args.Cancelled) {
                return;
            } else if (args.Error != null) {
                passwordHelp.Text = string.Format(Properties_Resources.LoginFailed, args.Error.Message);
            } else {
                passwordHelp.Text = Properties_Resources.LoginSuccess;
            }

            passwordProgress.Visibility = Visibility.Hidden;
            passwordChanged = false;
            passwordBox.IsEnabled = true;
        }

        private void CreateTreeView()
        {
            System.Uri resourceLocater = new System.Uri("/DataSpaceSync;component/FolderTreeMVC/TreeView.xaml", System.UriKind.Relative);
            treeView = Application.LoadComponent(resourceLocater) as TreeView;

            repo = new CmisSync.CmisTree.RootFolder()
            {
                Name = FolderName,
                Id = Credentials.RepoId,
                Address = Credentials.Address.ToString()
            };

            ObservableCollection<RootFolder> repos = new ObservableCollection<RootFolder>();
            repos.Add(repo);
            repo.Selected = true;

            asyncLoader = new AsyncNodeLoader(repo, Credentials, PredefinedNodeLoader.LoadSubFolderDelegate, PredefinedNodeLoader.CheckSubFolderDelegate);
            IgnoredFolderLoader.AddIgnoredFolderToRootNode(repo, Ignores);
            LocalFolderLoader.AddLocalFolderToRootNode(repo, localPath);
            asyncLoader.Load(repo);

            treeView.DataContext = repos;

            treeView.AddHandler(TreeViewItem.ExpandedEvent, new RoutedEventHandler(delegate(object sender, RoutedEventArgs e)
            {
                TreeViewItem expandedItem = e.OriginalSource as TreeViewItem;
                Node expandedNode = expandedItem.Header as Folder;
                if (expandedNode != null)
                {
                    asyncLoader.Load(expandedNode);
                }
            }));
        }


        private void LoadEdit()
        {
            System.Uri resourceLocater = new System.Uri("/DataSpaceSync;component/EditWPF.xaml", System.UriKind.Relative);
            UserControl editWPF = Application.LoadComponent(resourceLocater) as UserControl;

            tab = editWPF.FindName("tab") as TabControl;
            //  GUI workaround to remove ignore folder {{
            //tabItemSelection = editWPF.FindName("tabItemSelection") as TabItem;
            //  GUI workaround to remove ignore folder }}
            tabItemCredentials = editWPF.FindName("tabItemCredentials") as TabItem;
            addressLabel = editWPF.FindName("addressLabel") as TextBlock;
            addressBox = editWPF.FindName("addressBox") as TextBox;
            userLabel = editWPF.FindName("userLabel") as TextBlock;
            userBox = editWPF.FindName("userBox") as TextBox;
            passwordLabel = editWPF.FindName("passwordLabel") as TextBlock;
            passwordBox = editWPF.FindName("passwordBox") as PasswordBox;
            passwordProgress = editWPF.FindName("passwordProgress") as CircularProgressBar;
            passwordHelp = editWPF.FindName("passwordHelp") as TextBlock;
            finishButton = editWPF.FindName("finishButton") as Button;
            cancelButton = editWPF.FindName("cancelButton") as Button;

            //  GUI workaround to remove ignore folder {{
            //tabItemSelection.Content = treeView;
            //  GUI workaround to remove ignore folder }}

            addressBox.Text = Credentials.Address.ToString();
            userBox.Text = Credentials.UserName;
            passwordBox.Password = Credentials.Password.ToString();
            passwordHelp.Text = string.Empty;

            ContentCanvas.Children.Add(editWPF);

            passwordBox.LostFocus += delegate { backgroundWorker.RunWorkerAsync(); };
            passwordBox.PasswordChanged += delegate { passwordChanged = true; };
            passwordChanged = true;
            backgroundWorker.RunWorkerAsync();
        }
    }
}
