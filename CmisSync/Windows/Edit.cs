using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using CmisSync.Lib.Credentials;
using CmisSync.CmisTree;
using System.Collections.ObjectModel;

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

        private bool useXAML = true;

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

            CreateTreeView();

            if (useXAML)
            {
                LoadEdit();
            }
            else
            {
                CreateEdit();
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
        private Button finishButton;
        private Button cancelButton;


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
            asyncLoader = new AsyncNodeLoader(repo, Credentials, PredefinedNodeLoader.LoadSubFolderDelegate, PredefinedNodeLoader.CheckSubFolderDelegate);
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
        }


        private void LoadEdit()
        {
            System.Uri resourceLocater = new System.Uri("/DataSpaceSync;component/EditWPF.xaml", System.UriKind.Relative);
            UserControl editWPF = Application.LoadComponent(resourceLocater) as UserControl;

            tab = editWPF.FindName("tab") as TabControl;
            tabItemSelection = editWPF.FindName("tabItemSelection") as TabItem;
            tabItemCredentials = editWPF.FindName("tabItemCredentials") as TabItem;
            addressLabel = editWPF.FindName("addressLabel") as TextBlock;
            addressBox = editWPF.FindName("addressBox") as TextBox;
            userLabel = editWPF.FindName("userLabel") as TextBlock;
            userBox = editWPF.FindName("userBox") as TextBox;
            passwordLabel = editWPF.FindName("passwordLabel") as TextBlock;
            passwordBox = editWPF.FindName("passwordBox") as PasswordBox;
            finishButton = editWPF.FindName("finishButton") as Button;
            cancelButton = editWPF.FindName("cancelButton") as Button;

            tabItemSelection.Content = treeView;

            addressBox.Text = Credentials.Address.ToString();
            userBox.Text = Credentials.UserName;
            passwordBox.Password = Credentials.Password.ToString();

            ContentCanvas.Children.Add(editWPF);
        }


        /// <summary>
        /// Create the UI
        /// </summary>
        private void CreateEdit()
        {
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

            tab = new TabControl();

            tabItemSelection = new TabItem();
            tabItemSelection.Header = Properties_Resources.AddingFolder;
            tabItemSelection.Content = canvasSelection;
            tab.Items.Add(tabItemSelection);

            tabItemCredentials = new TabItem();
            tabItemCredentials.Header = Properties_Resources.Credentials;
            tabItemCredentials.Content = canvasCredentials;
            tab.Items.Add(tabItemCredentials);

            ContentCanvas.Children.Add(tab);
            Canvas.SetTop(tab, 30);
            Canvas.SetLeft(tab, 175);

            finishButton = new Button()
            {
                Content = Properties_Resources.SaveChanges,
                IsDefault = true
            };

            cancelButton = new Button()
            {
                Content = Properties_Resources.DiscardChanges,
                IsDefault = false
            };

            Buttons.Add(finishButton);
            Buttons.Add(cancelButton);

            finishButton.Focus();
        }
    }
}
