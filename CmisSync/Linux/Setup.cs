//-----------------------------------------------------------------------
// <copyright file="Setup.cs" company="GRAU DATA AG">
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

namespace CmisSync {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Timers;

    using CmisSync.CmisTree;
    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Cmis.UiUtils;
    using CmisSync.Lib.Config;

    using Gtk;

    using Mono.Unix;

    [CLSCompliant(false)]
    public class Setup : SetupWindow {
        private static Gdk.Cursor handCursor = new Gdk.Cursor(Gdk.CursorType.Hand2);
        private static Gdk.Cursor waitCursor = new Gdk.Cursor(Gdk.CursorType.Watch);
        private static Gdk.Cursor defaultCursor = new Gdk.Cursor(Gdk.CursorType.LeftPtr);

        private SetupController controller = new SetupController();
        private string cancelText = Properties_Resources.Cancel;
        private string continueText = Properties_Resources.Continue;
        private string backText = Properties_Resources.Back;

        private delegate Tuple<CmisServer, Exception> GetRepositoriesFuzzyDelegate(ServerCredentials credentials);

        private delegate string[] GetSubfoldersDelegate(
            string repositoryId,
            string path,
            string address,
            string user,
            string password);

        private void ShowSetupPage()
        {
            this.Header = string.Format(Properties_Resources.Welcome, Properties_Resources.ApplicationName);
            this.Description = string.Format(Properties_Resources.Intro, Properties_Resources.ApplicationName);

            this.Add(new Label(string.Empty)); // Page must have at least one element in order to show Header and Descripton

            Button cancel_button = new Button(this.cancelText);
            cancel_button.Clicked += delegate {
                this.controller.SetupPageCancelled();
            };

            Button continue_button = new Button(this.continueText)
            {
                Sensitive = false
            };

            continue_button.Clicked += delegate(object o, EventArgs args) {
                this.controller.SetupPageCompleted();
            };

            this.AddButton(cancel_button);
            this.AddButton(continue_button);

            this.controller.UpdateSetupContinueButtonEvent += delegate(bool button_enabled) {
                Application.Invoke(delegate {
                        continue_button.Sensitive = button_enabled;
                        });
            };

            this.controller.CheckSetupPage();
        }

        private void ShowAdd1Page()
        {
            this.Present();
            this.Header = Properties_Resources.Where;

            VBox layout_vertical   = new VBox(false, 12);
            HBox layout_fields     = new HBox(true, 12);
            VBox layout_address    = new VBox(true, 0);
            HBox layout_address_help = new HBox(false, 3);
            VBox layout_user       = new VBox(true, 0);
            VBox layout_password   = new VBox(true, 0);

            // Address
            Label address_label = new Label()
            {
                UseMarkup = true,
                Xalign = 0,
                Markup = "<b>" +
                Properties_Resources.EnterWebAddress +
                "</b>"
            };

            Entry address_entry = new Entry() {
                Text = (this.controller.PreviousAddress == null || string.IsNullOrEmpty(this.controller.PreviousAddress.ToString())) ? "https://" : this.controller.PreviousAddress.ToString(),
                ActivatesDefault = false
            };

            Label address_help_label = new Label()
            {
                Xalign = 0,
                UseMarkup = true,
                Markup = "<span foreground=\"#808080\" size=\"small\">" +
                Properties_Resources.Help + ": " +
                "</span>"
            };
            EventBox address_help_urlbox = new EventBox();
            Label address_help_urllabel = new Label()
            {
                Xalign = 0,
                UseMarkup = true,
                Markup = "<span foreground=\"blue\" underline=\"single\" size=\"small\">" +
                Properties_Resources.WhereToFind +
                "</span>"
            };
            address_help_urlbox.Add(address_help_urllabel);
            address_help_urlbox.ButtonPressEvent += delegate(object o, ButtonPressEventArgs args) {
                Process process = new Process();
                process.StartInfo.FileName  = "xdg-open";
                process.StartInfo.Arguments = "https://github.com/nicolas-raoul/CmisSync/wiki/What-address";
                process.Start();
            };
            address_help_urlbox.EnterNotifyEvent += delegate(object o, EnterNotifyEventArgs args) {
                address_help_urlbox.GdkWindow.Cursor = handCursor;
            };

            Label address_error_label = new Label()
            {
                Xalign = 0,
                UseMarkup = true,
                Markup = string.Empty
            };
            address_error_label.Hide();

            // User
            Entry user_entry = new Entry() {
                Text = this.controller.PreviousPath,
                ActivatesDefault = false
            };

            if (string.IsNullOrEmpty(this.controller.saved_user)) {
                user_entry.Text = Environment.UserName;
            } else {
                user_entry.Text = this.controller.saved_user;
            }

            // Password
            Entry password_entry = new Entry() {
                Visibility = false,
                ActivatesDefault = true
            };

            this.controller.ChangeAddressFieldEvent += delegate(string text, string example_text) {
                Application.Invoke(delegate {
                    address_entry.Text = text;
                });
            };

            this.controller.ChangeUserFieldEvent += delegate(string text, string example_text) {
                Application.Invoke(delegate {
                    user_entry.Text = text;
                });
            };

            this.controller.ChangePasswordFieldEvent += delegate(string text, string example_text) {
                Application.Invoke(delegate {
                    password_entry.Text = text;
                });
            };

            address_entry.Changed += delegate {
                string error = this.controller.CheckAddPage(address_entry.Text);
                if (!string.IsNullOrEmpty(error)) {
                    address_error_label.Markup = "<span foreground=\"red\">" + Properties_Resources.ResourceManager.GetString(error, CultureInfo.CurrentCulture) + "</span>";
                    address_error_label.Show();
                } else {
                    address_error_label.Hide();
                }
            };

            // Address
            layout_address_help.PackStart(address_help_label, false, false, 0);
            layout_address_help.PackStart(address_help_urlbox, false, false, 0);
            layout_address.PackStart(address_label, true, true, 0);
            layout_address.PackStart(address_entry, true, true, 0);
            layout_address.PackStart(layout_address_help, true, true, 0);

            // User
            layout_user.PackStart(
                new Label() {
                Markup = "<b>" + Properties_Resources.User + ":</b>",
                Xalign = 0
            },
            true,
            true,
            0);
            layout_user.PackStart(user_entry, false, false, 0);

            // Password
            layout_password.PackStart(
                new Label() {
                Markup = "<b>" + Properties_Resources.Password + ":</b>",
                Xalign = 0
            },
            true,
            true,
            0);
            layout_password.PackStart(password_entry, false, false, 0);
            layout_fields.PackStart(layout_user);
            layout_fields.PackStart(layout_password);
            layout_vertical.PackStart(layout_address, false, false, 0);
            layout_vertical.PackStart(layout_fields, false, false, 0);
            layout_vertical.PackStart(address_error_label, true, true, 0);
            this.Add(layout_vertical);

            // Cancel button
            Button cancel_button = new Button(this.cancelText);

            cancel_button.Clicked += delegate {
                this.controller.PageCancelled();
            };

            // Continue button
            Button continue_button = new Button(this.continueText) {
                Sensitive = string.IsNullOrEmpty(this.controller.CheckAddPage(address_entry.Text))
            };

            continue_button.Clicked += delegate {
                // Show wait cursor
                this.GdkWindow.Cursor = waitCursor;

                // Try to find the CMIS server (asynchronous using a delegate)
                GetRepositoriesFuzzyDelegate dlgt =
                    new GetRepositoriesFuzzyDelegate(CmisUtils.GetRepositoriesFuzzy);
                ServerCredentials credentials = new ServerCredentials() {
                    UserName = user_entry.Text,
                    Password = password_entry.Text,
                    Address = new Uri(address_entry.Text)
                };
                IAsyncResult ar = dlgt.BeginInvoke(credentials, null, null);
                while (!ar.AsyncWaitHandle.WaitOne(100)) {
                    while (Application.EventsPending()) {
                        Application.RunIteration();
                    }
                }

                Tuple<CmisServer, Exception> result = dlgt.EndInvoke(ar);
                CmisServer cmisServer = result.Item1;
                if (cmisServer != null) {
                    this.controller.repositories = cmisServer.Repositories;
                    address_entry.Text = cmisServer.Url.ToString();
                } else {
                    this.controller.repositories = null;
                }

                // Hide wait cursor
                this.GdkWindow.Cursor = defaultCursor;

                if (this.controller.repositories == null) {
                    // Show warning
                    string warning = this.controller.GetConnectionsProblemWarning(cmisServer, result.Item2);
                    address_error_label.Markup = "<span foreground=\"red\">" + warning + "</span>";
                    address_error_label.Show();
                } else {
                    // Continue to folder selection
                    this.controller.Add1PageCompleted(
                        new Uri(address_entry.Text), user_entry.Text, password_entry.Text);
                }
            };

            this.controller.UpdateAddProjectButtonEvent += delegate(bool button_enabled) {
                Application.Invoke(delegate {
                    continue_button.Sensitive = button_enabled;
                    if(button_enabled) {
                        continue_button.SetFlag(Gtk.WidgetFlags.CanFocus);
                        continue_button.SetFlag(Gtk.WidgetFlags.CanDefault);
                        continue_button.GrabDefault();
                    }
                });
            };

            this.AddButton(cancel_button);
            this.AddButton(continue_button);

            this.controller.CheckAddPage(address_entry.Text);
            address_entry.GrabFocus();
        }

        private void ShowAdd2Page()
        {
            CmisTreeStore cmisStore = new CmisTreeStore();
            Gtk.TreeView treeView = new Gtk.TreeView(cmisStore);

            bool firstRepo = true;
            List<RootFolder> repositories = new List<RootFolder>();
            Dictionary<string, AsyncNodeLoader> loader = new Dictionary<string, AsyncNodeLoader>();
            foreach (KeyValuePair<string, string> repository in this.controller.repositories)
            {
                RootFolder root = new RootFolder() {
                    Name = repository.Value,
                    Id = repository.Key,
                    Address = this.controller.saved_address.ToString()
                };
                if (firstRepo) {
                    root.Selected = true;
                    firstRepo = false;
                } else {
                    root.Selected = false;
                }

                repositories.Add(root);
                CmisRepoCredentials cred = new CmisRepoCredentials() {
                    UserName = this.controller.saved_user,
                    Password = this.controller.saved_password,
                    Address = this.controller.saved_address,
                    RepoId = repository.Key
                };
                AsyncNodeLoader asyncLoader = new AsyncNodeLoader(root, cred, PredefinedNodeLoader.LoadSubFolderDelegate, PredefinedNodeLoader.CheckSubFolderDelegate);
                asyncLoader.UpdateNodeEvent += delegate {
                    cmisStore.UpdateCmisTree(root);
                };
                cmisStore.UpdateCmisTree(root);
            }

            this.Header = Properties_Resources.Which;

            VBox layout_vertical   = new VBox(false, 12);

            Button cancel_button = new Button(this.cancelText);
            cancel_button.Clicked += delegate {
                foreach (AsyncNodeLoader task in loader.Values) {
                    task.Cancel();
                }

                this.controller.PageCancelled();
            };

            Button continue_button = new Button(this.continueText) {
                Sensitive = repositories.Count > 0
            };

            continue_button.Clicked += delegate {
                RootFolder root = repositories.Find(x => (x.Selected != false));
                if (root != null)
                {
                    foreach (AsyncNodeLoader task in loader.Values) {
                        task.Cancel();
                    }

                    this.controller.saved_repository = root.Id;
                    List<string> ignored = NodeModelUtils.GetIgnoredFolder(root);
                    List<string> selected = NodeModelUtils.GetSelectedFolder(root);
                    this.controller.Add2PageCompleted(root.Id, root.Path, ignored.ToArray(), selected.ToArray());
                }
            };

            Button back_button = new Button(this.backText) {
                Sensitive = true
            };

            back_button.Clicked += delegate {
                foreach (AsyncNodeLoader task in loader.Values) {
                    task.Cancel();
                }

                this.controller.BackToPage1();
            };

            treeView.HeadersVisible = false;
            treeView.Selection.Mode = SelectionMode.Single;

            TreeViewColumn column = new TreeViewColumn();
            column.Title = "Name";
            CellRendererToggle renderToggle = new CellRendererToggle();
            column.PackStart(renderToggle, false);
            renderToggle.Activatable = true;
            column.AddAttribute(renderToggle, "active", (int)CmisTreeStore.Column.ColumnSelected);
            column.AddAttribute(renderToggle, "inconsistent", (int)CmisTreeStore.Column.ColumnSelectedThreeState);
            column.AddAttribute(renderToggle, "radio", (int)CmisTreeStore.Column.ColumnRoot);
            renderToggle.Toggled += delegate(object render, ToggledArgs args) {
                TreeIter iterToggled;
                if (!cmisStore.GetIterFromString(out iterToggled, args.Path)) {
                    Console.WriteLine("Toggled GetIter Error " + args.Path);
                    return;
                }

                Node node = cmisStore.GetValue(iterToggled, (int)CmisTreeStore.Column.ColumnNode) as Node;
                if (node == null) {
                    Console.WriteLine("Toggled GetValue Error " + args.Path);
                    return;
                }

                RootFolder selectedRoot = repositories.Find(x => (x.Selected != false));
                Node parent = node;
                while (parent.Parent != null) {
                    parent = parent.Parent;
                }

                RootFolder root = parent as RootFolder;
                if (root != selectedRoot) {
                    selectedRoot.Selected = false;
                    cmisStore.UpdateCmisTree(selectedRoot);
                }

                if (node.Parent == null) {
                    node.Selected = true;
                } else {
                    node.Selected = !node.Selected;
                }

                cmisStore.UpdateCmisTree(root);
            };
            CellRendererText renderText = new CellRendererText();
            column.PackStart(renderText, false);
            column.SetAttributes(renderText, "text", (int)CmisTreeStore.Column.ColumnName);
            column.Expand = true;
            treeView.AppendColumn(column);

            treeView.RowExpanded += delegate(object o, RowExpandedArgs args) {
                Node node = cmisStore.GetValue(args.Iter, (int)CmisTreeStore.Column.ColumnNode) as Node;
                Node parent = node;
                while (parent.Parent != null) {
                    parent = parent.Parent;
                }

                RootFolder root = parent as RootFolder;
                loader[root.Id].Load(node);
            };

            ScrolledWindow sw = new ScrolledWindow() {
                ShadowType = Gtk.ShadowType.In
            };
            sw.Add(treeView);

            layout_vertical.PackStart(new Label(string.Empty), false, false, 0);
            layout_vertical.PackStart(sw, true, true, 0);
            this.Add(layout_vertical);
            this.AddButton(back_button);
            this.AddButton(cancel_button);
            this.AddButton(continue_button);

            if (repositories.Count > 0) {
                continue_button.GrabDefault();
                continue_button.GrabFocus();
            } else {
                back_button.GrabDefault();
                back_button.GrabFocus();
            }
        }

        private void ShowCustomizePage()
        {
            this.Header = Properties_Resources.Customize;
            string localfoldername = this.controller.saved_address.Host.ToString();
            foreach (KeyValuePair<string, string> repository in this.controller.repositories) {
                if (repository.Key == this.controller.saved_repository) {
                    localfoldername += "/" + repository.Value;
                    break;
                }
            }

            Label localfolder_label = new Label() {
                Xalign = 0,
                UseMarkup = true,
                Markup = "<b>" + Properties_Resources.EnterLocalFolderName + "</b>"
            };

            Entry localfolder_entry = new Entry() {
                Text = localfoldername,
                     ActivatesDefault = false
            };

            Label localrepopath_label = new Label() {
                Xalign = 0,
                UseMarkup = true,
                Markup = "<b>" + Properties_Resources.ChangeRepoPath + "</b>"
            };

            Entry localrepopath_entry = new Entry() {
                Text = System.IO.Path.Combine(this.controller.DefaultRepoPath, localfolder_entry.Text)
            };

            localfolder_entry.Changed += delegate {
                try {
                    localrepopath_entry.Text = System.IO.Path.Combine(this.controller.DefaultRepoPath, localfolder_entry.Text);
                } catch(Exception) {
                }
            };

            Label localfolder_error_label = new Label() {
                Xalign = 0,
                       UseMarkup = true,
                       Markup = string.Empty
            };

            Button cancel_button = new Button(this.cancelText);
            Button add_button = new Button(Properties_Resources.Add);
            Button back_button = new Button(Properties_Resources.Back);

            this.controller.UpdateAddProjectButtonEvent += delegate(bool button_enabled) {
                Gtk.Application.Invoke(delegate {
                    add_button.Sensitive = button_enabled;
                });
            };

            string error = this.controller.CheckRepoPathAndName(localrepopath_entry.Text, localfolder_entry.Text);
            if (!string.IsNullOrEmpty(error)) {
                localfolder_error_label.Markup = "<span foreground=\"#ff8080\">" +
                    error +
                    "</span>";
                localfolder_error_label.Show();
            } else {
                localfolder_error_label.Hide();
            }

            localfolder_entry.Changed += delegate {
                error = this.controller.CheckRepoPathAndName(localrepopath_entry.Text, localfolder_entry.Text);
                if (!string.IsNullOrEmpty(error)) {
                    localfolder_error_label.Markup = "<span foreground=\"#ff8080\">" +
                        error + "</span>";
                    localfolder_error_label.Show();
                } else {
                    localfolder_error_label.Hide();
                }
            };

            error = this.controller.CheckRepoPathAndName(localrepopath_entry.Text, localfolder_entry.Text);
            if (!string.IsNullOrEmpty(error)) {
                localfolder_error_label.Markup = "<span foreground=\"#ff8080\">" +
                    error +
                "</span>";
                localfolder_error_label.Show();
            } else {
                localfolder_error_label.Hide();
            }

            localrepopath_entry.Changed += delegate {
                error = this.controller.CheckRepoPathAndName(localrepopath_entry.Text, localfolder_entry.Text);
                if (!string.IsNullOrEmpty(error)) {
                    localfolder_error_label.Markup = "<span foreground=\"#ff8080\">" +
                        error +
                        "</span>";
                    localfolder_error_label.Show();
                } else {
                    localfolder_error_label.Hide();
                }
            };

            cancel_button.Clicked += delegate {
                this.controller.PageCancelled();
            };

            back_button.Clicked += delegate {
                this.controller.BackToPage2();
            };

            add_button.Clicked += delegate {
                this.controller.CustomizePageCompleted(localfolder_entry.Text, localrepopath_entry.Text);
            };

            VBox layout_vertical   = new VBox(false, 12);

            layout_vertical.PackStart(new Label(string.Empty), false, false, 0);
            layout_vertical.PackStart(localfolder_label, true, true, 0);
            layout_vertical.PackStart(localfolder_entry, true, true, 0);
            layout_vertical.PackStart(localrepopath_label, true, true, 0);
            layout_vertical.PackStart(localrepopath_entry, true, true, 0);
            layout_vertical.PackStart(localfolder_error_label, true, true, 0);
            this.Add(layout_vertical);
            this.AddButton(back_button);
            this.AddButton(cancel_button);
            this.AddButton(add_button);
            localfolder_entry.GrabFocus();
            localfolder_entry.SelectRegion(0, localfolder_entry.Text.Length);
        }

        private void ShowSyncingPage()
        {
            this.Header = Properties_Resources.AddingFolder
                + " ‘" + this.controller.SyncingReponame + "’…";
            this.Description = Properties_Resources.MayTakeTime;

            Button finish_button = new Button() {
                Sensitive = false,
                Label = "Finish"
            };

            this.AddButton(finish_button);
        }

        private void ShowFinishedPage()
        {
            this.UrgencyHint = true;

            this.Header = Properties_Resources.Ready;
            this.Description = string.Format(Properties_Resources.YouCanFind, this.controller.saved_local_path);

            // A button that opens the synced folder
            Button open_folder_button = new Button(string.Format(
                "Open {0}",
                System.IO.Path.GetFileName(this.controller.PreviousPath)));

            open_folder_button.Clicked += delegate {
                this.controller.OpenFolderClicked();
            };

            Button finish_button = new Button(Properties_Resources.Finish);

            finish_button.Clicked += delegate {
                this.controller.FinishPageCompleted();
            };

            this.Add(new Label(string.Empty));
            this.AddButton(open_folder_button);
            this.AddButton(finish_button);
        }

        private void ShowTutorialPage()
        {
            switch (this.controller.TutorialCurrentPage) {
            case 1:
            {
                this.Header = Properties_Resources.WhatsNext;
                this.Description = string.Format(Properties_Resources.CmisSyncCreates, Properties_Resources.ApplicationName);

                Button skip_tutorial_button = new Button(Properties_Resources.SkipTutorial);
                skip_tutorial_button.Clicked += delegate {
                    this.controller.TutorialSkipped();
                };

                Button continue_button = new Button(this.continueText);
                continue_button.Clicked += delegate {
                    this.controller.TutorialPageCompleted();
                };

                Image slide = UIHelpers.GetImage("tutorial-slide-1.png");
                this.Add(slide);
                this.AddButton(skip_tutorial_button);
                this.AddButton(continue_button);
                break;
            }

            case 2:
            {
                this.Header = Properties_Resources.Synchronization;
                this.Description = Properties_Resources.DocumentsAre;

                Button continue_button = new Button(this.continueText);
                continue_button.Clicked += delegate {
                    this.controller.TutorialPageCompleted();
                };

                Image slide = UIHelpers.GetImage("tutorial-slide-2.png");
                this.Add(slide);
                this.AddButton(continue_button);
                break;
            }

            case 3:
            {
                this.Header = Properties_Resources.StatusIcon;
                this.Description = string.Format(Properties_Resources.StatusIconShows, Properties_Resources.ApplicationName);

                Button continue_button = new Button(this.continueText);
                continue_button.Clicked += delegate {
                    this.controller.TutorialPageCompleted();
                };

                Image slide = UIHelpers.GetImage("tutorial-slide-3.png");
                this.Add(slide);
                this.AddButton(continue_button);
                break;
            }

            case 4:
            {
                this.Header = string.Format(Properties_Resources.AddFolders, Properties_Resources.ApplicationName);
                this.Description = Properties_Resources.YouCan;
                Image slide = UIHelpers.GetImage("tutorial-slide-4.png");

                Button finish_button = new Button(Properties_Resources.Finish);
                finish_button.Clicked += delegate {
                    this.controller.TutorialPageCompleted();
                };

                CheckButton check_button = new CheckButton(string.Format(Properties_Resources.Startup, Properties_Resources.ApplicationName)) {
                    Active = true
                };

                check_button.Toggled += delegate {
                    this.controller.StartupItemChanged(check_button.Active);
                };
                this.Add(slide);
                this.AddOption(check_button);
                this.AddButton(finish_button);
                break;
            }
            }
        }

        public Setup() : base()
        {
            this.controller.HideWindowEvent += delegate {
                Application.Invoke(delegate {
                    this.HideAll();
                        });
            };

            this.controller.ShowWindowEvent += delegate {
                Application.Invoke(delegate {
                    this.ShowAll();
                    this.Present();
                });
            };

            this.controller.ChangePageEvent += delegate(PageType ptype) {
                Application.Invoke(delegate {
                    this.Reset();

                    switch (ptype) {
                    case PageType.Setup:
                        this.ShowSetupPage();
                        break;

                    case PageType.Add1:
                        this.ShowAdd1Page();
                        break;

                    case PageType.Add2:
                        this.ShowAdd2Page();
                        break;

                    case PageType.Customize:
                        this.ShowCustomizePage();
                        break;

                    case PageType.Finished:
                        this.ShowFinishedPage();
                        break;

                    case PageType.Tutorial:
                        this.ShowTutorialPage();
                        break;
                    }

                    this.ShowAll();
                });
            };
            this.DeleteEvent += delegate
            {
                this.controller.PageCancelled();
            };
        }

        private void RenderServiceColumn(
            TreeViewColumn column,
            CellRenderer cell,
            TreeModel model,
            TreeIter iter)
        {
            string markup = (string)model.GetValue(iter, 1);
            TreeSelection selection = (column.TreeView as TreeView).Selection;

            if (selection.IterIsSelected(iter)) {
                if (column.TreeView.HasFocus) {
                    markup = markup.Replace(this.SecondaryTextColor, this.SecondaryTextColorSelected);
                } else {
                    markup = markup.Replace(this.SecondaryTextColorSelected, this.SecondaryTextColor);
                }
            } else {
                markup = markup.Replace(this.SecondaryTextColorSelected, this.SecondaryTextColor);
            }

            (cell as CellRendererText).Markup = markup;
        }
    }
}