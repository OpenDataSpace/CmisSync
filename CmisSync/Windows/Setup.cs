//   CmisSync, a collaboration and sharing tool.
//   Copyright (C) 2010  Hylke Bons <hylkebons@gmail.com>
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
//   along with this program. If not, see <http://www.gnu.org/licenses/>.


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shell;

using Drawing = System.Drawing;
using Imaging = System.Windows.Interop.Imaging;
using WPF = System.Windows.Controls;

using CmisSync.Lib.Cmis;
using CmisSync.Lib;
using System.Globalization;
using log4net;
using System.Collections.ObjectModel;
using CmisSync.CmisTree;
using System.Windows.Input;
using CmisSync.Lib.Credentials;

namespace CmisSync
{
    /// <summary>
    /// Dialog for the tutorial, and for the wizard to add a new remote folder.
    /// </summary>
    public class Setup : SetupWindow
    {
        protected static readonly ILog Logger = LogManager.GetLogger(typeof(Setup));

        /// <summary>
        /// MVC controller.
        /// </summary>
        public SetupController Controller = new SetupController();

        delegate Tuple<CmisServer, Exception> GetRepositoriesFuzzyDelegate(ServerCredentials credentials);

        // Public Buttons
        private Button continue_button;
        private Button cancel_button;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Setup()
        {
            Logger.Info("Entering constructor.");

            // Defines how to show the setup window.
            Controller.ShowWindowEvent += delegate
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    Logger.Info("Entering ShowWindowEvent.");
                    Show();
                    Activate();
                    BringIntoView();
                    Logger.Info("Exiting ShowWindowEvent.");
                });
            };

            // Defines how to hide the setup windows.
            Controller.HideWindowEvent += delegate
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    Hide();
                });
            };

            // Defines what to do when changing page.
            // The remote folder addition wizard has several steps.
            Controller.ChangePageEvent += delegate(PageType type)
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    Logger.Info("Entering ChangePageEvent.");
                    Reset();

                    // Show appropriate setup page.
                    switch (type)
                    {
                        // Welcome page that shows up at first run.
                        case PageType.Setup:
                            //SetupWelcome();
                            LoadWelcomeWPF();
                            break;

                        case PageType.Tutorial:
                            //SetupTutorial();
                            LoadTutorialWFP();
                            break;

                        // First step of the remote folder addition dialog: Specifying the server.
                        case PageType.Add1:
                            //SetupAddLogin();
                            LoadAddLoginWPF();
                            break;

                        // Second step of the remote folder addition dialog: choosing the folder.
                        case PageType.Add2:
                            SetupAddSelectRepo();
                            break;

                        // Third step of the remote folder addition dialog: Customizing the local folder.
                        case PageType.Customize:
                            SetupAddCustomize();
                            break;

                        // Fourth page of the remote folder addition dialog: starting to sync.
                        // TODO: This step should be removed. Now it appears just a brief instant, because sync is asynchronous.
                        case PageType.Syncing:
                            SetupAddSyncing();
                            break;

                        // Final page of the remote folder addition dialog: end of the addition wizard.
                        case PageType.Finished:
                            SetupAddFinish();
                            break;
                    }

                    ShowAll();
                    Logger.Info("Exiting ChangePageEvent.");
                });
            };
            this.Closing += delegate
            {
                Controller.PageCancelled();
            };

            Controller.PageCancelled();
            Logger.Info("Exiting constructor.");
        }

        private void LoadWelcomeWPF()
        {
            // UI elements.
            Header = String.Format(Properties_Resources.Welcome, Properties_Resources.ApplicationName);
            Description = String.Format(Properties_Resources.Intro, Properties_Resources.ApplicationName);

            System.Uri resourceLocater = new System.Uri("/DataSpaceSync;component/SetupWelcomeWPF.xaml", System.UriKind.Relative);
            UserControl LoadXAML = Application.LoadComponent(resourceLocater) as UserControl;

            continue_button = LoadXAML.FindName("continue_button") as Button;
            cancel_button = LoadXAML.FindName("cancel_button") as Button;

            ContentCanvas.Children.Add(LoadXAML);

            // Actions.
            Controller.UpdateSetupContinueButtonEvent += delegate(bool enabled)
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    continue_button.IsEnabled = enabled;
                });
            };

            cancel_button.Click += delegate
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    Program.UI.StatusIcon.Dispose();
                    Controller.SetupPageCancelled();
                });
            };

            continue_button.Click += delegate
            {
                Controller.SetupPageCompleted();
            };

            Controller.CheckSetupPage();
        }

        private WPF.Image slide_image;
        private CheckBox check_box;

        private void LoadTutorialWFP()
        {
            switch (Controller.TutorialCurrentPage)
            {
                // First page of the tutorial.
                case 1:
                    {
                        // UI elements.
                        Header = Properties_Resources.WhatsNext;
                        Description = String.Format(Properties_Resources.CmisSyncCreates, Properties_Resources.ApplicationName);

                        System.Uri resourceLocater = new System.Uri("/DataSpaceSync;component/SetupTutorialFirstWPF.xaml", System.UriKind.Relative);
                        UserControl LoadXAML = Application.LoadComponent(resourceLocater) as UserControl;

                        slide_image = LoadXAML.FindName("slide_image") as WPF.Image;
                        continue_button = LoadXAML.FindName("continue_button") as Button;
                        cancel_button = LoadXAML.FindName("cancel_button") as Button;

                        ContentCanvas.Children.Add(LoadXAML);

                        // Actions.
                        cancel_button.Click += delegate
                        {
                            Controller.TutorialSkipped();
                        };

                        continue_button.Click += delegate
                        {
                            Controller.TutorialPageCompleted();
                        };

                        break;
                    }

                // Second page of the tutorial.
                case 2:
                    {
                        // UI elements.
                        Header = Properties_Resources.Synchronization;
                        Description = Properties_Resources.DocumentsAre;

                        System.Uri resourceLocater = new System.Uri("/DataSpaceSync;component/SetupTutorialSecondWPF.xaml", System.UriKind.Relative);
                        UserControl LoadXAML = Application.LoadComponent(resourceLocater) as UserControl;

                        slide_image = LoadXAML.FindName("slide_image") as WPF.Image;
                        continue_button = LoadXAML.FindName("continue_button") as Button;

                        ContentCanvas.Children.Add(LoadXAML);

                        // Actions.
                        continue_button.Click += delegate
                        {
                            Controller.TutorialPageCompleted();
                        };

                        break;
                    }

                // Third page of the tutorial.
                case 3:
                    {
                        // UI elements.
                        Header = Properties_Resources.StatusIcon;
                        Description = String.Format(
                            Properties_Resources.StatusIconShows,
                            Properties_Resources.ApplicationName);

                        System.Uri resourceLocater = new System.Uri("/DataSpaceSync;component/SetupTutorialThirdWPF.xaml", System.UriKind.Relative);
                        UserControl LoadXAML = Application.LoadComponent(resourceLocater) as UserControl;

                        slide_image = LoadXAML.FindName("slide_image") as WPF.Image;
                        continue_button = LoadXAML.FindName("continue_button") as Button;

                        ContentCanvas.Children.Add(LoadXAML);

                        // Actions.
                        continue_button.Click += delegate
                        {
                            Controller.TutorialPageCompleted();
                        };

                        break;
                    }

                // Fourth page of the tutorial.
                case 4:
                    {
                        // UI elements.
                        Header = String.Format(Properties_Resources.AddFolders, Properties_Resources.ApplicationName);
                        Description = Properties_Resources.YouCan;

                        System.Uri resourceLocater = new System.Uri("/DataSpaceSync;component/SetupTutorialFourthWPF.xaml", System.UriKind.Relative);
                        UserControl LoadXAML = Application.LoadComponent(resourceLocater) as UserControl;

                        slide_image = LoadXAML.FindName("slide_image") as WPF.Image;
                        continue_button = LoadXAML.FindName("continue_button") as Button;
                        check_box = LoadXAML.FindName("check_box") as WPF.CheckBox;
                        
                        ContentCanvas.Children.Add(LoadXAML);

                        // Actions.
                        check_box.Click += delegate
                        {
                            Controller.StartupItemChanged(check_box.IsChecked.Value);
                        };

                        continue_button.Click += delegate
                        {
                            Controller.TutorialPageCompleted();
                        };

                        break;
                    }
            }
        }

        private void ControllerChangeAddressAction(string text, string example_text)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                address_box.Text = text;
                address_help_label.Text = example_text;
            });
        }

        private void ControllerChangeUserAction(string text, string example_text)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                user_box.Text = text;
                user_help_label.Text = example_text;
            });
        }

        private void ControllerChangePasswordAction(string text, string example_text)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                password_box.Password = text;
                password_help_label.Text = example_text;
            });
        }

        private void ControllerLoginAddProjectAction(bool button_enabled)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                continue_button.IsEnabled = button_enabled;
            });
        }

        private void ControllerLoginInsertAction()
        {
            Controller.ChangeAddressFieldEvent += ControllerChangeAddressAction;
            Controller.ChangeUserFieldEvent += ControllerChangeUserAction;
            Controller.ChangePasswordFieldEvent += ControllerChangePasswordAction;
            Controller.UpdateAddProjectButtonEvent += ControllerLoginAddProjectAction;
        }

        private void ControllerLoginRemoveAction()
        {
            Controller.ChangeAddressFieldEvent -= ControllerChangeAddressAction;
            Controller.ChangeUserFieldEvent -= ControllerChangeUserAction;
            Controller.ChangePasswordFieldEvent -= ControllerChangePasswordAction;
            Controller.UpdateAddProjectButtonEvent -= ControllerLoginAddProjectAction;
        }

        // LoadAddLogin
        private TextBlock address_label;
        private TextBox address_box;
        private TextBlock address_help_label;
        private TextBlock user_label;
        private TextBox user_box;
        private TextBlock user_help_label;
        private TextBlock password_label;
        private PasswordBox password_box;
        private TextBlock password_help_label;
        private TextBox address_error_label;

        private void LoadAddLoginWPF()
        {
            // define UI elements.
            Header = Properties_Resources.Where;

            System.Uri resourceLocater = new System.Uri("/DataSpaceSync;component/SetupAddLoginWPF.xaml", System.UriKind.Relative);
            UserControl LoadAddLoginWPF = Application.LoadComponent(resourceLocater) as UserControl;

            address_label = LoadAddLoginWPF.FindName("address_label") as TextBlock;
            address_box = LoadAddLoginWPF.FindName("address_box") as TextBox;
            address_help_label = LoadAddLoginWPF.FindName("address_help_label") as TextBlock;
            user_label = LoadAddLoginWPF.FindName("user_label") as TextBlock;
            user_box = LoadAddLoginWPF.FindName("user_box") as TextBox;
            user_help_label = LoadAddLoginWPF.FindName("user_help_label") as TextBlock;
            password_label = LoadAddLoginWPF.FindName("password_label") as TextBlock;
            password_box = LoadAddLoginWPF.FindName("password_box") as PasswordBox;
            password_help_label = LoadAddLoginWPF.FindName("password_help_label") as TextBlock;
            address_error_label = LoadAddLoginWPF.FindName("address_error_label") as TextBox;
            continue_button = LoadAddLoginWPF.FindName("continue_button") as Button;
            cancel_button = LoadAddLoginWPF.FindName("cancel_button") as Button;

            ContentCanvas.Children.Add(LoadAddLoginWPF);

            address_box.Text = (Controller.PreviousAddress != null) ? Controller.PreviousAddress.ToString() : String.Empty;

            if (Controller.saved_user == String.Empty || Controller.saved_user == null)
            {
                user_box.Text = Environment.UserName;
            }
            else
            {
                user_box.Text = Controller.saved_user;
            }

            TaskbarItemInfo.ProgressValue = 0.0;
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;

            if (Controller.PreviousAddress == null || Controller.PreviousAddress.ToString() == String.Empty)
                address_box.Text = "https://";
            else
                address_box.Text = Controller.PreviousAddress.ToString();
            address_box.Focus();
            address_box.Select(address_box.Text.Length, 0);

            // Actions.
            ControllerLoginInsertAction();
            Controller.CheckAddPage(address_box.Text);

            address_box.TextChanged += delegate
            {
                string error = Controller.CheckAddPage(address_box.Text);
                if (!String.IsNullOrEmpty(error))
                {
                    address_error_label.Text = Properties_Resources.ResourceManager.GetString(error, CultureInfo.CurrentCulture);
                    address_error_label.Visibility = Visibility.Visible;
                }
                else address_error_label.Visibility = Visibility.Hidden;
            };

            cancel_button.Click += delegate
            {
                ControllerLoginRemoveAction();
                Controller.PageCancelled();
            };

            continue_button.Click += delegate
            {
                // Show wait cursor
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

                // Try to find the CMIS server (asynchronously)
                GetRepositoriesFuzzyDelegate dlgt =
                    new GetRepositoriesFuzzyDelegate(CmisUtils.GetRepositoriesFuzzy);
                ServerCredentials credentials = new ServerCredentials()
                {
                    UserName = user_box.Text,
                    Password = password_box.Password,
                    Address = new Uri(address_box.Text)
                };
                IAsyncResult ar = dlgt.BeginInvoke(credentials, null, null);
                while (!ar.AsyncWaitHandle.WaitOne(100))
                {
                    System.Windows.Forms.Application.DoEvents();
                }
                Tuple<CmisServer, Exception> result = dlgt.EndInvoke(ar);
                CmisServer cmisServer = result.Item1;

                Controller.repositories = cmisServer != null ? cmisServer.Repositories : null;

                address_box.Text = cmisServer.Url.ToString();

                // Hide wait cursor
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

                if (Controller.repositories == null)
                {
                    // Could not retrieve repositories list from server, show warning.
                    string warning = Controller.GetConnectionsProblemWarning(cmisServer, result.Item2);
                    address_error_label.Text = warning;
                    address_error_label.Visibility = Visibility.Visible;
                }
                else
                {
                    ControllerLoginRemoveAction();
                    // Continue to next step, which is choosing a particular folder.
                    Controller.Add1PageCompleted(
                        new Uri(address_box.Text), user_box.Text, password_box.Password);
                }
            };
        }


        private void SetupAddSelectRepo()
        {
            // UI elements.
            Header = Properties_Resources.Which;

            // A tree allowing the user to browse CMIS repositories/folders.
            System.Uri resourceLocater = new System.Uri("/DataSpaceSync;component/FolderTreeMVC/TreeView.xaml", System.UriKind.Relative);
            System.Windows.Controls.TreeView treeView = System.Windows.Application.LoadComponent(resourceLocater) as TreeView;

            ObservableCollection<RootFolder> repos = new ObservableCollection<RootFolder>();
            Dictionary<string, AsyncNodeLoader> loader = new Dictionary<string, AsyncNodeLoader>();
            // Some CMIS servers hold several repositories (ex:Nuxeo). Show one root per repository.
            bool firstRepo = true;
            foreach (KeyValuePair<String, String> repository in Controller.repositories)
            {
                RootFolder repo = new RootFolder()
                {
                    Name = repository.Value,
                    Id = repository.Key,
                    Address = Controller.saved_address.ToString()
                };
                repos.Add(repo);
                if (firstRepo)
                {
                    repo.Selected = true;
                    firstRepo = false;
                }
                else
                {
                    repo.Selected = false;
                }
                CmisRepoCredentials cred = new CmisRepoCredentials()
                {
                    UserName = Controller.saved_user,
                    Password = Controller.saved_password,
                    Address = Controller.saved_address,
                    RepoId = repository.Key
                };
                AsyncNodeLoader asyncLoader = new AsyncNodeLoader(repo, cred, PredefinedNodeLoader.LoadSubFolderDelegate, PredefinedNodeLoader.CheckSubFolderDelegate);
                asyncLoader.Load(repo);
                loader.Add(repo.Id, asyncLoader);
            }
            treeView.DataContext = repos;
            treeView.AddHandler(TreeViewItem.ExpandedEvent, new RoutedEventHandler(delegate(object sender, RoutedEventArgs e)
            {
                TreeViewItem expandedItem = e.OriginalSource as TreeViewItem;
                Node expandedNode = expandedItem.Header as Folder;
                if (expandedNode != null)
                {
                    Node parent = expandedNode.Parent;
                    while (parent is Folder)
                    {
                        parent = parent.Parent;
                    }
                    if (parent is RootFolder)
                    {
                        AsyncNodeLoader l;
                        RootFolder r = parent as RootFolder;
                        if (loader.TryGetValue(r.Id, out l))
                        {
                            l.Load(expandedNode);
                        }
                    }
                }
            }));
            ContentCanvas.Children.Add(treeView);
            Canvas.SetTop(treeView, 70);
            Canvas.SetLeft(treeView, 185);

            Button continue_button = new Button()
            {
                Content = Properties_Resources.Continue,
                IsEnabled = !firstRepo,
                IsDefault = true
            };
            Button cancel_button = new Button()
            {
                Content = Properties_Resources.Cancel
            };
            Button back_button = new Button()
            {
                Content = Properties_Resources.Back,
                IsDefault = false
            };
            Buttons.Add(back_button);
            Buttons.Add(continue_button);
            Buttons.Add(cancel_button);
            continue_button.Focus();

            cancel_button.Click += delegate
            {
                Controller.PageCancelled();
            };

            continue_button.Click += delegate
            {
                List<string> ignored = new List<string>();
                List<string> selectedFolder = new List<string>();
                ItemCollection items = treeView.Items;
                RootFolder selectedRepo = null;
                foreach (var item in items)
                {
                    RootFolder repo = item as RootFolder;
                    if (repo != null)
                        if (repo.Selected != false)
                        {
                            selectedRepo = repo;
                            break;
                        }
                }
                if (selectedRepo != null)
                {
                    ignored.AddRange(NodeModelUtils.GetIgnoredFolder(selectedRepo));
                    selectedFolder.AddRange(NodeModelUtils.GetSelectedFolder(selectedRepo));
                    Controller.saved_repository = selectedRepo.Id;
                    Controller.saved_remote_path = selectedRepo.Path;
                    Controller.Add2PageCompleted(
                        Controller.saved_repository, Controller.saved_remote_path, ignored.ToArray(), selectedFolder.ToArray());
                    foreach (AsyncNodeLoader task in loader.Values)
                        task.Cancel();
                }
                else
                {
                    return;
                }
            };

            back_button.Click += delegate
            {
                Controller.BackToPage1();
                foreach (AsyncNodeLoader task in loader.Values)
                    task.Cancel();
            };

            Controller.HideWindowEvent += delegate
            {
                foreach (AsyncNodeLoader task in loader.Values)
                    task.Cancel();
            };
        }

        private void SetupAddCustomize()
        {
            string parentFolder = Controller.DefaultRepoPath;

            // UI elements.
            Header = Properties_Resources.Customize;



            // Customize local folder name
            TextBlock localfolder_label = new TextBlock()
            {
                Text = Properties_Resources.EnterLocalFolderName,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                Width = 420
            };
            string localfoldername = Controller.saved_address.Host.ToString();
            foreach (KeyValuePair<String, String> repository in Controller.repositories)
            {
                if (repository.Key == Controller.saved_repository)
                {
                    localfoldername += "\\" + repository.Value;
                    break;
                }
            }
            TextBox localfolder_box = new TextBox()
            {
                Width = 420,
                Text = localfoldername
            };
            TextBlock localrepopath_label = new TextBlock()
            {
                Text = Properties_Resources.ChangeRepoPath,
                FontWeight = FontWeights.Bold
            };
            TextBox localrepopath_box = new TextBox()
            {
                Width = 375,
                Text = Path.Combine(parentFolder, localfolder_box.Text)
            };
            localfolder_box.TextChanged += delegate
            {
                try
                {
                    localrepopath_box.Text = Path.Combine(parentFolder, localfolder_box.Text);
                }
                catch (Exception)
                { }
            };
            Button choose_folder_button = new Button()
            {
                Width = 40,
                Content = "..."
            };
            TextBlock localfolder_error_label = new TextBlock()
            {
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 128, 128)),
                Visibility = Visibility.Hidden,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 420
            };

            Button cancel_button = new Button()
            {
                Content = Properties_Resources.Cancel
            };
            Button add_button = new Button()
            {
                Content = Properties_Resources.Add,
                IsDefault = true
            };
            Button back_button = new Button()
            {
                Content = Properties_Resources.Back
            };
            Buttons.Add(back_button);
            Buttons.Add(add_button);
            Buttons.Add(cancel_button);

            // Local Folder Name
            ContentCanvas.Children.Add(localfolder_label);
            Canvas.SetTop(localfolder_label, 160);
            Canvas.SetLeft(localfolder_label, 185);

            ContentCanvas.Children.Add(localfolder_box);
            Canvas.SetTop(localfolder_box, 180);
            Canvas.SetLeft(localfolder_box, 185);

            ContentCanvas.Children.Add(localrepopath_label);
            Canvas.SetTop(localrepopath_label, 200);
            Canvas.SetLeft(localrepopath_label, 185);

            ContentCanvas.Children.Add(localrepopath_box);
            Canvas.SetTop(localrepopath_box, 220);
            Canvas.SetLeft(localrepopath_box, 185);

            ContentCanvas.Children.Add(choose_folder_button);
            Canvas.SetTop(choose_folder_button, 220);
            Canvas.SetLeft(choose_folder_button, 565);

            ContentCanvas.Children.Add(localfolder_error_label);
            Canvas.SetTop(localfolder_error_label, 275);
            Canvas.SetLeft(localfolder_error_label, 185);

            TaskbarItemInfo.ProgressValue = 0.0;
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;

            localfolder_box.Focus();
            localfolder_box.Select(localfolder_box.Text.Length, 0);

            // Repo path validity.

            CheckCustomizeInput(localfolder_box, localrepopath_box, localfolder_error_label);

            // Actions.

            Controller.UpdateAddProjectButtonEvent += delegate(bool button_enabled)
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    if (add_button.IsEnabled != button_enabled)
                    {
                        add_button.IsEnabled = button_enabled;
                        if (button_enabled)
                        {
                            add_button.IsDefault = true;
                            back_button.IsDefault = false;
                        }
                    }
                });
            };

            localfolder_box.TextChanged += delegate
            {
                CheckCustomizeInput(localfolder_box, localrepopath_box, localfolder_error_label);
            };

            localrepopath_box.TextChanged += delegate
            {
                CheckCustomizeInput(localfolder_box, localrepopath_box, localfolder_error_label);
            };

            // Choose a folder.
            choose_folder_button.Click += delegate
            {
                System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
                if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    parentFolder = folderBrowserDialog1.SelectedPath;
                    localrepopath_box.Text = Path.Combine(parentFolder, localfolder_box.Text);
                }
            };

            // Other actions.

            cancel_button.Click += delegate
            {
                Controller.PageCancelled();
            };

            back_button.Click += delegate
            {
                Controller.BackToPage2();
            };

            add_button.Click += delegate
            {
                Controller.CustomizePageCompleted(localfolder_box.Text, localrepopath_box.Text);
            };

            Controller.LocalPathExists += LocalPathExistsHandler;
        }

        private void SetupAddSyncing()
        {
            // UI elements.
            Header = Properties_Resources.AddingFolder + " ‘" + Controller.SyncingReponame + "’…";
            Description = Properties_Resources.MayTakeTime;

            ProgressBar progress_bar = new ProgressBar()
            {
                Width = 414,
                Height = 15,
                Value = Controller.ProgressBarPercentage
            };

            ContentCanvas.Children.Add(progress_bar);
            Canvas.SetLeft(progress_bar, 185);
            Canvas.SetTop(progress_bar, 150);

            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;

            Button finish_button = new Button()
            {
                Content = Properties_Resources.Finish,
                IsEnabled = false
            };
            Buttons.Add(finish_button);

            // Actions.

            Controller.UpdateProgressBarEvent += delegate(double percentage)
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    progress_bar.Value = percentage;
                    TaskbarItemInfo.ProgressValue = percentage / 100;
                });
            };
        }

        private void SetupAddFinish()
        {
            // UI elements.
            Header = Properties_Resources.Ready;
            Description = String.Format(Properties_Resources.YouCanFind, Properties_Resources.ApplicationName);

            TaskbarItemInfo.ProgressValue = 0.0;
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
            
            Button finish_button = new Button()
            {
                Content = Properties_Resources.Finish
            };
            Button open_folder_button = new Button()
            {
                Content = Properties_Resources.OpenFolder
            };
            Buttons.Add(open_folder_button);
            Buttons.Add(finish_button);

            // Actions.
            finish_button.Click += delegate
            {
                Controller.FinishPageCompleted();
            };

            open_folder_button.Click += delegate
            {
                Controller.OpenFolderClicked();
            };

            SystemSounds.Exclamation.Play();
        }

        private static bool LocalPathExistsHandler(string path) {
            return System.Windows.MessageBox.Show(String.Format(
                    Properties_Resources.ConfirmExistingLocalFolderText, path),
                    String.Format(Properties_Resources.ConfirmExistingLocalFolderTitle, path),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No
                    ) == MessageBoxResult.Yes;
        }

        private void CheckCustomizeInput(TextBox localfolder_box, TextBox localrepopath_box, TextBlock localfolder_error_label)
        {
            string error = Controller.CheckRepoPathAndName(localrepopath_box.Text, localfolder_box.Text);
            if (!String.IsNullOrEmpty(error))
            {
                localfolder_error_label.Text = error;
                localfolder_error_label.Visibility = Visibility.Visible;
                localfolder_error_label.Foreground = Brushes.Red;
            }
            else
            {
                try
                {
                    Controller.CheckRepoPathExists(localrepopath_box.Text);
                    localfolder_error_label.Visibility = Visibility.Hidden;
                }
                catch (ArgumentException e)
                {
                    localfolder_error_label.Visibility = Visibility.Visible;
                    localfolder_error_label.Foreground = Brushes.Orange;
                    localfolder_error_label.Text = e.Message;
                }
            }
        }
    }

    /// <summary>
    /// Stores the metadata of an item in the folder selection dialog.
    /// </summary>
    public class SelectionTreeItem
    {
        /// <summary>
        /// Whether this item's children have been loaded yet.
        /// </summary>
        public bool childrenLoaded = false;

        /// <summary>
        /// Address of the repository.
        /// Only necessary for repository root nodes.
        /// </summary>
        public string repository;

        /// <summary>
        /// Full path to the item.
        /// </summary>
        public string fullPath;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SelectionTreeItem(string repository, string fullPath)
        {
            this.repository = repository;
            this.fullPath = fullPath;
        }
    }
}
