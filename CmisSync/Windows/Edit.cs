using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using CmisSync.Lib.Credentials;
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

            CreateEdit();


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
        }


        protected override void Close(object sender, CancelEventArgs args)
        {
            Controller.CloseWindow();
        }

        private TextBlock addressLabel;
        private TextBox addressBox;
        private TextBlock userLabel;
        private TextBox userBox;
        private TextBlock passwordLabel;
        private PasswordBox passwordBox;
        private CircularProgressBar loggingProgress;
        private TextBlock passwordHelp;
        private bool passwordChanged = false;

        private void CheckPassword()
        {
            if (!passwordChanged)
            {
                return;
            }

            passwordHelp.Text = Properties_Resources.LoginCheck;
            passwordBox.IsEnabled = false;
            ServerCredentials cred = new ServerCredentials()
            {
                Address = Credentials.Address,
                UserName = Credentials.UserName,
                Password = passwordBox.Password
            };
            new TaskFactory().StartNew(() =>
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    loggingProgress.Visibility = Visibility.Visible;
                });
                string output;
                try
                {
                    CmisSync.Lib.Cmis.CmisUtils.GetRepositories(cred);
                    output = Properties_Resources.LoginSuccess;
                }
                catch (Exception e)
                {
                    output = string.Format(Properties_Resources.LoginFailed, e.Message);
                }
                Dispatcher.BeginInvoke((Action)delegate
                {
                    passwordChanged = false;
                    passwordHelp.Text = output;
                    passwordBox.IsEnabled = true;
                    loggingProgress.Visibility = Visibility.Hidden;
                });
            });
        }

        /// <summary>
        /// Create the UI
        /// </summary>
        private void CreateEdit()
        {
            System.Uri resourceLocater = new System.Uri("/DataSpaceSync;component/FolderTreeMVC/TreeView.xaml", System.UriKind.Relative);
            TreeView treeView = Application.LoadComponent(resourceLocater) as TreeView;

            CmisSync.CmisTree.RootFolder repo = new CmisSync.CmisTree.RootFolder()
            {
                Name = FolderName,
                Id = Credentials.RepoId,
                Address = Credentials.Address.ToString()
            };
            AsyncNodeLoader asyncLoader = new AsyncNodeLoader(repo, Credentials, PredefinedNodeLoader.LoadSubFolderDelegate, PredefinedNodeLoader.CheckSubFolderDelegate);
            IgnoredFolderLoader.AddIgnoredFolderToRootNode(repo, Ignores);
            LocalFolderLoader.AddLocalFolderToRootNode(repo, localPath);

            asyncLoader.Load(repo);

            ObservableCollection<RootFolder> repos = new ObservableCollection<RootFolder>();
            repos.Add(repo);
            repo.Selected = true;

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

            addressLabel = new TextBlock()
            {
                Text = Properties_Resources.CmisWebAddress + ":",
                FontWeight = FontWeights.Bold
            };
            addressBox = new TextBox()
            {
                Width = 410,
                Text = this.Credentials.Address.ToString(),
                IsEnabled = false
            };
            userLabel = new TextBlock()
            {
                Width = 200,
                Text = Properties_Resources.User + ":",
                FontWeight = FontWeights.Bold
            };
            userBox = new TextBox()
            {
                Width = 200,
                Text = this.Credentials.UserName,
                IsEnabled = false
            };
            passwordLabel = new TextBlock()
            {
                Width = 200,
                Text = Properties_Resources.Password + ":",
                FontWeight = FontWeights.Bold
            };
            passwordBox = new PasswordBox()
            {
                Width = 200,
                Password = this.Credentials.Password.ToString()
            };
            loggingProgress = new CircularProgressBar();
            passwordHelp = new TextBlock()
            {
                Width = 200,
            };

            Canvas canvasSelection = new Canvas();
            canvasSelection.Width = 430;
            canvasSelection.Height = 287;
            canvasSelection.Children.Add(treeView);

            Canvas canvasCredentials = new Canvas();
            canvasCredentials.Width = 430;
            canvasCredentials.Height = 287;
            canvasCredentials.Children.Add(addressLabel);
            Canvas.SetTop(addressLabel, 40);
            Canvas.SetLeft(addressLabel, 10);
            canvasCredentials.Children.Add(addressBox);
            Canvas.SetTop(addressBox, 60);
            Canvas.SetLeft(addressBox, 10);
            canvasCredentials.Children.Add(userLabel);
            Canvas.SetTop(userLabel, 100);
            Canvas.SetLeft(userLabel, 10);
            canvasCredentials.Children.Add(userBox);
            Canvas.SetTop(userBox, 120);
            Canvas.SetLeft(userBox, 10);
            canvasCredentials.Children.Add(passwordLabel);
            Canvas.SetTop(passwordLabel, 100);
            Canvas.SetLeft(passwordLabel, 220);
            canvasCredentials.Children.Add(passwordBox);
            Canvas.SetTop(passwordBox, 120);
            Canvas.SetLeft(passwordBox, 220);
            canvasCredentials.Children.Add(loggingProgress);
            Canvas.SetTop(loggingProgress, 120);
            Canvas.SetLeft(loggingProgress, 400);
            canvasCredentials.Children.Add(passwordHelp);
            Canvas.SetTop(passwordHelp, 140);
            Canvas.SetLeft(passwordHelp, 220);

            TabControl tab = new TabControl();

            TabItem tabItemSelection = new TabItem();
            tabItemSelection.Header = Properties_Resources.AddingFolder;
            tabItemSelection.Content = canvasSelection;
            tab.Items.Add(tabItemSelection);

            TabItem tabItemCredentials = new TabItem();
            tabItemCredentials.Header = Properties_Resources.Credentials;
            tabItemCredentials.Content = canvasCredentials;
            tab.Items.Add(tabItemCredentials);

            switch (type)
            {
                case EditType.EditFolder:
                    tab.SelectedItem = tabItemSelection;
                    break;
                case EditType.EditCredentials:
                    tab.SelectedItem = tabItemCredentials;
                    break;
                default:
                    break;
            }

            ContentCanvas.Children.Add(tab);
            Canvas.SetTop(tab, 30);
            Canvas.SetLeft(tab, 175);

            passwordBox.LostFocus += delegate { CheckPassword(); };
            passwordBox.PasswordChanged += delegate { passwordChanged = true; };
            passwordChanged = true;
            CheckPassword();

            Controller.CloseWindowEvent += delegate
            {
                asyncLoader.Cancel();
            };


            Button finish_button = new Button()
            {
                Content = Properties_Resources.SaveChanges,
                IsDefault = false
            };

            Button cancel_button = new Button()
            {
                Content = Properties_Resources.DiscardChanges,
                IsDefault = false
            };

            Buttons.Add(finish_button);
            Buttons.Add(cancel_button);

            finish_button.Focus();

            finish_button.Click += delegate
            {
                Ignores = NodeModelUtils.GetIgnoredFolder(repo);
                Credentials.Password = passwordBox.Password;
                Controller.SaveFolder();
                Close();
            };

            cancel_button.Click += delegate
            {
                Close();
            };
            this.Title = Properties_Resources.EditTitle;
            this.Description = "";
            this.ShowAll();
        }
    }
}
