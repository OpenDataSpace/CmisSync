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


using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Timers;
using System.Collections.Generic;
using System.Globalization;

using Gtk;
using Mono.Unix;

using CmisSync.Lib;
using CmisSync.Lib.Cmis;
using CmisSync.Lib.Credentials;
using CmisSync.CmisTree;

namespace CmisSync {
 
    [CLSCompliant(false)]
    public class Setup : SetupWindow {

        public SetupController Controller = new SetupController ();

        private ProgressBar progress_bar = new ProgressBar ();

        private static Gdk.Cursor hand_cursor = new Gdk.Cursor(Gdk.CursorType.Hand2);
        private static Gdk.Cursor wait_cursor = new Gdk.Cursor(Gdk.CursorType.Watch);
        private static Gdk.Cursor default_cursor = new Gdk.Cursor(Gdk.CursorType.LeftPtr);

        private string cancelText =
            Properties_Resources.Cancel;
        private string continueText =
            Properties_Resources.Continue;
        private string backText =
            Properties_Resources.Back;

        delegate Tuple<CmisServer, Exception> GetRepositoriesFuzzyDelegate(ServerCredentials credentials);

        delegate string[] GetSubfoldersDelegate(string repositoryId, string path,
            string address, string user, string password);

        private void ShowSetupPage()
        {
            Header = String.Format(Properties_Resources.Welcome, Properties_Resources.ApplicationName);
            Description = String.Format(Properties_Resources.Intro, Properties_Resources.ApplicationName);

            Add(new Label("")); // Page must have at least one element in order to show Header and Descripton

            Button cancel_button = new Button (cancelText);
            cancel_button.Clicked += delegate {
                Controller.SetupPageCancelled ();
            };

            Button continue_button = new Button (continueText)
            {
                Sensitive = false
            };

            continue_button.Clicked += delegate (object o, EventArgs args) {
                Controller.SetupPageCompleted ();
            };

            AddButton (cancel_button);
            AddButton (continue_button);

            Controller.UpdateSetupContinueButtonEvent += delegate (bool button_enabled) {
                Application.Invoke (delegate {
                        continue_button.Sensitive = button_enabled;
                        });
            };

            Controller.CheckSetupPage ();
        }

        private void ShowAdd1Page()
        {
            this.Present();
            Header = Properties_Resources.Where;

            VBox layout_vertical   = new VBox (false, 12);
            HBox layout_fields     = new HBox (true, 12);
            VBox layout_address    = new VBox (true, 0);
            HBox layout_address_help = new HBox(false, 3);
            VBox layout_user       = new VBox (true, 0);
            VBox layout_password   = new VBox (true, 0);

            // Address
            Label address_label = new Label()
            {
                UseMarkup = true,
                          Xalign = 0,
                          Markup = "<b>" + 
                              Properties_Resources.EnterWebAddress +
                              "</b>"
            };

            Entry address_entry = new Entry () {
                Text = (Controller.PreviousAddress == null || String.IsNullOrEmpty(Controller.PreviousAddress.ToString()))?"https://":Controller.PreviousAddress.ToString(),
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
                process.Start ();
            };
            address_help_urlbox.EnterNotifyEvent += delegate(object o, EnterNotifyEventArgs args) {
                address_help_urlbox.GdkWindow.Cursor = hand_cursor;
            };

            Label address_error_label = new Label()
            {
                Xalign = 0,
                UseMarkup = true,
                Markup = ""
            };
            address_error_label.Hide();

            // User
            Entry user_entry = new Entry () {
                Text = Controller.PreviousPath,
                     ActivatesDefault = false
            };

            if(String.IsNullOrEmpty(Controller.saved_user))
            {
                user_entry.Text = Environment.UserName;
            }
            else
            {
                user_entry.Text = Controller.saved_user;
            }

            // Password
            Entry password_entry = new Entry () {
                Visibility = false,
                ActivatesDefault = true
            };

            Controller.ChangeAddressFieldEvent += delegate (string text,
                    string example_text) {

                Application.Invoke (delegate {
                        address_entry.Text      = text;
                        });
            };

            Controller.ChangeUserFieldEvent += delegate (string text,
                    string example_text) {

                Application.Invoke (delegate {
                        user_entry.Text      = text;
                        });
            };

            Controller.ChangePasswordFieldEvent += delegate (string text,
                    string example_text) {

                Application.Invoke (delegate {
                        password_entry.Text      = text;
                        });
            };

            address_entry.Changed += delegate {
                string error = Controller.CheckAddPage(address_entry.Text);
                if (!String.IsNullOrEmpty(error)) {
                    address_error_label.Markup = "<span foreground=\"red\">" + Properties_Resources.ResourceManager.GetString(error, CultureInfo.CurrentCulture) + "</span>";
                    address_error_label.Show();
                } else {
                    address_error_label.Hide();
                }
            };

            // Address
            layout_address_help.PackStart(address_help_label, false, false, 0);
            layout_address_help.PackStart(address_help_urlbox, false, false, 0);
            layout_address.PackStart (address_label, true, true, 0);
            layout_address.PackStart (address_entry, true, true, 0);
            layout_address.PackStart (layout_address_help, true, true, 0);
//            layout_address.PackStart (address_error_label, true, true, 0);

            // User
            layout_user.PackStart (new Label () {
                    Markup = "<b>" + Properties_Resources.User + ":</b>",
                    Xalign = 0
                    }, true, true, 0);
            layout_user.PackStart (user_entry, false, false, 0);

            // Password
            layout_password.PackStart (new Label () {
                    Markup = "<b>" + Properties_Resources.Password + ":</b>",
                    Xalign = 0
                    }, true, true, 0);
            layout_password.PackStart (password_entry, false, false, 0);

            layout_fields.PackStart (layout_user);
            layout_fields.PackStart (layout_password);

//            layout_vertical.PackStart (new Label (""), false, false, 0);
            layout_vertical.PackStart (layout_address, false, false, 0);
            layout_vertical.PackStart (layout_fields, false, false, 0);
            layout_vertical.PackStart (address_error_label, true, true, 0);

            Add (layout_vertical);

            // Cancel button
            Button cancel_button = new Button (cancelText);

            cancel_button.Clicked += delegate {
                Controller.PageCancelled ();
            };

            // Continue button
            Button continue_button = new Button (continueText) {
                Sensitive = String.IsNullOrEmpty( Controller.CheckAddPage (address_entry.Text))
            };

            continue_button.Clicked += delegate {
                // Show wait cursor
                this.GdkWindow.Cursor = wait_cursor;

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
                if(cmisServer != null)
                {
                    Controller.repositories = cmisServer.Repositories;
                    address_entry.Text = cmisServer.Url.ToString();
                }
                else
                {
                    Controller.repositories = null;
                }
                // Hide wait cursor
                this.GdkWindow.Cursor = default_cursor;

                if (Controller.repositories == null)
                {
                    // Show warning
                    string warning = Controller.GetConnectionsProblemWarning(cmisServer, result.Item2);
                    address_error_label.Markup = "<span foreground=\"red\">" + warning + "</span>";
                    address_error_label.Show();
                }
                else
                {
                    // Continue to folder selection
                    Controller.Add1PageCompleted(
                            new Uri(address_entry.Text), user_entry.Text, password_entry.Text);
                }
            };

            Controller.UpdateAddProjectButtonEvent += delegate (bool button_enabled) {
                Application.Invoke (delegate {
                    continue_button.Sensitive = button_enabled;
                    if(button_enabled) {
                        continue_button.SetFlag(Gtk.WidgetFlags.CanFocus);
                        continue_button.SetFlag(Gtk.WidgetFlags.CanDefault);
                        continue_button.GrabDefault();
                    }
                });
            };

            AddButton (cancel_button);
            AddButton (continue_button);

            Controller.CheckAddPage (address_entry.Text);
            address_entry.GrabFocus ();
        }

        private void ShowAdd2Page()
        {
            CmisTreeStore cmisStore = new CmisTreeStore();
            Gtk.TreeView treeView = new Gtk.TreeView(cmisStore);

            bool firstRepo = true;
            List<RootFolder> repositories = new List<RootFolder>();
            Dictionary<string,AsyncNodeLoader> loader = new Dictionary<string, AsyncNodeLoader> ();
            foreach (KeyValuePair<String, String> repository in Controller.repositories)
            {
                RootFolder root = new RootFolder () {
                    Name = repository.Value,
                    Id = repository.Key,
                    Address = Controller.saved_address.ToString()
                };
                if (firstRepo)
                {
                    root.Selected = true;
                    firstRepo = false;
                }
                else
                {
                    root.Selected = false;
                }
                repositories.Add (root);
                CmisRepoCredentials cred = new CmisRepoCredentials () {
                    UserName = Controller.saved_user,
                    Password = Controller.saved_password,
                    Address = Controller.saved_address,
                    RepoId = repository.Key
                };
                AsyncNodeLoader asyncLoader = new AsyncNodeLoader (root, cred, PredefinedNodeLoader.LoadSubFolderDelegate, PredefinedNodeLoader.CheckSubFolderDelegate);
                asyncLoader.UpdateNodeEvent += delegate {
                    cmisStore.UpdateCmisTree(root);
                };
                cmisStore.UpdateCmisTree (root);
                asyncLoader.Load (root);
                loader.Add (root.Id, asyncLoader);
            }

            Header = Properties_Resources.Which;

            VBox layout_vertical   = new VBox (false, 12);

            Button cancel_button = new Button (cancelText);
            cancel_button.Clicked += delegate {
                foreach (AsyncNodeLoader task in loader.Values)
                    task.Cancel();
                Controller.PageCancelled ();
            };

            Button continue_button = new Button (continueText)
            {
                Sensitive = (repositories.Count > 0)
            };

            continue_button.Clicked += delegate {
                RootFolder root = repositories.Find (x => (x.Selected != false));
                if (root != null)
                {
                    foreach (AsyncNodeLoader task in loader.Values)
                        task.Cancel();
                    Controller.saved_repository = root.Id;
                    List<string> ignored = NodeModelUtils.GetIgnoredFolder(root);
                    List<string> selected = NodeModelUtils.GetSelectedFolder(root);
                    Controller.Add2PageCompleted (root.Id, root.Path, ignored.ToArray(), selected.ToArray());
                }
            };

            Button back_button = new Button (backText)
            {
                Sensitive = true
            };

            back_button.Clicked += delegate {
                foreach (AsyncNodeLoader task in loader.Values)
                    task.Cancel();
                Controller.BackToPage1();
            };

            treeView.HeadersVisible = false;
            treeView.Selection.Mode = SelectionMode.Single;

            TreeViewColumn column = new TreeViewColumn ();
            column.Title = "Name";
            CellRendererToggle renderToggle = new CellRendererToggle ();
            column.PackStart (renderToggle, false);
            renderToggle.Activatable = true;
            column.AddAttribute (renderToggle, "active", (int)CmisTreeStore.Column.ColumnSelected);
            column.AddAttribute (renderToggle, "inconsistent", (int)CmisTreeStore.Column.ColumnSelectedThreeState);
            column.AddAttribute (renderToggle, "radio", (int)CmisTreeStore.Column.ColumnRoot);
            renderToggle.Toggled += delegate (object render, ToggledArgs args) {
                TreeIter iterToggled;
                if (! cmisStore.GetIterFromString (out iterToggled, args.Path))
                {
                    Console.WriteLine("Toggled GetIter Error " + args.Path);
                    return;
                }

                Node node = cmisStore.GetValue(iterToggled,(int)CmisTreeStore.Column.ColumnNode) as Node;
                if (node == null)
                {
                    Console.WriteLine("Toggled GetValue Error " + args.Path);
                    return;
                }

                RootFolder selectedRoot = repositories.Find (x => (x.Selected != false));
                Node parent = node;
                while (parent.Parent != null)
                {
                    parent = parent.Parent;
                }
                RootFolder root = parent as RootFolder;
                if (root != selectedRoot)
                {
                    selectedRoot.Selected = false;
                    cmisStore.UpdateCmisTree(selectedRoot);
                }

                if (node.Parent == null)
                {
                    node.Selected = true;
                }
                else
                {
                    if (node.Selected == false)
                    {
                        node.Selected = true;
                    }
                    else
                    {
                        node.Selected = false;
                    }
                }
                cmisStore.UpdateCmisTree(root);
            };
            CellRendererText renderText = new CellRendererText ();
            column.PackStart (renderText, false);
            column.SetAttributes (renderText, "text", (int)CmisTreeStore.Column.ColumnName);
            column.Expand = true;
            treeView.AppendColumn (column);

            treeView.AppendColumn ("Status", new StatusCellRenderer (), "text", (int)CmisTreeStore.Column.ColumnStatus);

            treeView.RowExpanded += delegate (object o, RowExpandedArgs args) {
                Node node = cmisStore.GetValue(args.Iter, (int)CmisTreeStore.Column.ColumnNode) as Node;
                Node parent = node;
                while (parent.Parent != null)
                {
                    parent = parent.Parent;
                }
                RootFolder root = parent as RootFolder;
                loader[root.Id].Load(node);
            };

            ScrolledWindow sw = new ScrolledWindow() {
                ShadowType = Gtk.ShadowType.In
            };
            sw.Add(treeView);

            layout_vertical.PackStart (new Label(""), false, false, 0);
            layout_vertical.PackStart (sw, true, true, 0);
            Add(layout_vertical);
            AddButton(back_button);
            AddButton(cancel_button);
            AddButton(continue_button);

            if (repositories.Count > 0)
            {
                continue_button.GrabDefault ();
                continue_button.GrabFocus ();
            }
            else
            {
                back_button.GrabDefault ();
                back_button.GrabFocus ();
            }
        }

        private void ShowCustomizePage()
        {
            Header = Properties_Resources.Customize;
            string localfoldername = Controller.saved_address.Host.ToString();
            foreach (KeyValuePair<String, String> repository in Controller.repositories)
            {
                                    if (repository.Key == Controller.saved_repository)
                                    {
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
                Text = System.IO.Path.Combine(Controller.DefaultRepoPath, localfolder_entry.Text)
            };

            localfolder_entry.Changed += delegate {
                try{
                    localrepopath_entry.Text = System.IO.Path.Combine(Controller.DefaultRepoPath, localfolder_entry.Text);
                }catch(Exception){}
            };

            Label localfolder_error_label = new Label() {
                Xalign = 0,
                       UseMarkup = true,
                       Markup = ""
            };

            Button cancel_button = new Button(cancelText);

            Button add_button = new Button(
                    Properties_Resources.Add
                    );

            Button back_button = new Button(
                    Properties_Resources.Back
                    );

            Controller.UpdateAddProjectButtonEvent += delegate(bool button_enabled) {
                Gtk.Application.Invoke(delegate {
                        add_button.Sensitive = button_enabled;
                        });
            };

            string error = Controller.CheckRepoPathAndName(localrepopath_entry.Text, localfolder_entry.Text);
            if (!String.IsNullOrEmpty(error)) {
                localfolder_error_label.Markup = "<span foreground=\"#ff8080\">" +
                    error +
                    "</span>";
                localfolder_error_label.Show();
            } else {
                localfolder_error_label.Hide();
            }

            localfolder_entry.Changed += delegate {
                error = Controller.CheckRepoPathAndName(localrepopath_entry.Text, localfolder_entry.Text);
                if (!String.IsNullOrEmpty(error)) {
                    localfolder_error_label.Markup = "<span foreground=\"#ff8080\">" +
                        error +
                        "</span>";
                    localfolder_error_label.Show();
                } else {
                    localfolder_error_label.Hide();
                }
            };

            error = Controller.CheckRepoPathAndName(localrepopath_entry.Text, localfolder_entry.Text);
            if (!String.IsNullOrEmpty(error)) {
                localfolder_error_label.Markup = "<span foreground=\"#ff8080\">" +
                    error +
                "</span>";
                localfolder_error_label.Show();
            } else {
                localfolder_error_label.Hide();
            }

            localrepopath_entry.Changed += delegate {
                error = Controller.CheckRepoPathAndName(localrepopath_entry.Text, localfolder_entry.Text);
                if (!String.IsNullOrEmpty(error)) {
                    localfolder_error_label.Markup = "<span foreground=\"#ff8080\">" +
                        error +
                        "</span>";
                    localfolder_error_label.Show();
                } else {
                    localfolder_error_label.Hide();
                }
            };

            cancel_button.Clicked += delegate {
                Controller.PageCancelled();
            };

            back_button.Clicked += delegate {
                Controller.BackToPage2();
            };

            add_button.Clicked += delegate {
                Controller.CustomizePageCompleted(localfolder_entry.Text, localrepopath_entry.Text);
            };

            VBox layout_vertical   = new VBox (false, 12);

            layout_vertical.PackStart (new Label(""), false, false, 0);
            layout_vertical.PackStart (localfolder_label, true, true, 0);
            layout_vertical.PackStart (localfolder_entry, true, true, 0);
            layout_vertical.PackStart (localrepopath_label, true, true, 0);
            layout_vertical.PackStart (localrepopath_entry, true, true, 0);
            layout_vertical.PackStart (localfolder_error_label, true, true, 0);
            Add(layout_vertical);
            AddButton(back_button);
            AddButton(cancel_button);
            AddButton(add_button);
            // add_button.GrabFocus();
            localfolder_entry.GrabFocus();
            localfolder_entry.SelectRegion(0, localfolder_entry.Text.Length);

        }

        private void ShowSyncingPage()
        {
            Header = Properties_Resources.AddingFolder
                + " ‘" + Controller.SyncingReponame + "’…";
            Description = Properties_Resources.MayTakeTime;

            this.progress_bar.Fraction = Controller.ProgressBarPercentage / 100;

            Button finish_button = new Button () {
                Sensitive = false,
                          Label = "Finish"
            };

            Controller.UpdateProgressBarEvent += delegate (double percentage) {
                Application.Invoke (delegate {
                        this.progress_bar.Fraction = percentage / 100;
                        });
            };

            if (this.progress_bar.Parent != null) {
                (this.progress_bar.Parent as Container).Remove (this.progress_bar);
            }

            VBox bar_wrapper = new VBox (false, 0);
            bar_wrapper.PackStart (this.progress_bar, false, false, 15);

            Add (bar_wrapper);
            AddButton (finish_button);

        }

        private void ShowFinishedPage()
        {
            UrgencyHint = true;

            Header = Properties_Resources.Ready;
            Description = String.Format(Properties_Resources.YouCanFind, Controller.saved_local_path);

            // A button that opens the synced folder
            Button open_folder_button = new Button (string.Format ("Open {0}",
                        System.IO.Path.GetFileName (Controller.PreviousPath)));

            open_folder_button.Clicked += delegate {
                Controller.OpenFolderClicked ();
            };

            Button finish_button = new Button (Properties_Resources.Finish);

            finish_button.Clicked += delegate {
                Controller.FinishPageCompleted ();
            };

            Add(new Label(""));
            AddButton (open_folder_button);
            AddButton (finish_button);

            //System.Media.SystemSounds.Exclamation.Play();
        }

        private void ShowTutorialPage()
        {
            switch (Controller.TutorialCurrentPage) {
                case 1:
                    {
                        Header = Properties_Resources.WhatsNext;
                        Description = String.Format(Properties_Resources.CmisSyncCreates, Properties_Resources.ApplicationName);

                        Button skip_tutorial_button = new Button (Properties_Resources.SkipTutorial);
                        skip_tutorial_button.Clicked += delegate {
                            Controller.TutorialSkipped ();
                        };

                        Button continue_button = new Button (continueText);
                        continue_button.Clicked += delegate {
                            Controller.TutorialPageCompleted ();
                        };

                        Image slide = UIHelpers.GetImage ("tutorial-slide-1.png");

                        Add (slide);

                        AddButton (skip_tutorial_button);
                        AddButton (continue_button);

                    }
                    break;

                case 2:
                    {
                        Header      = Properties_Resources.Synchronization;
                        Description = Properties_Resources.DocumentsAre;

                        Button continue_button = new Button (continueText);
                        continue_button.Clicked += delegate {
                            Controller.TutorialPageCompleted ();
                        };

                        Image slide = UIHelpers.GetImage ("tutorial-slide-2.png");

                        Add (slide);
                        AddButton (continue_button);

                    }
                    break;

                case 3:
                    {
                        Header      = Properties_Resources.StatusIcon;
                        Description = String.Format(Properties_Resources.StatusIconShows, Properties_Resources.ApplicationName);

                        Button continue_button = new Button (continueText);
                        continue_button.Clicked += delegate {
                            Controller.TutorialPageCompleted ();
                        };

                        Image slide = UIHelpers.GetImage ("tutorial-slide-3.png");

                        Add (slide);
                        AddButton (continue_button);

                    }
                    break;

                case 4:
                    {
                        Header      = String.Format (Properties_Resources.AddFolders, Properties_Resources.ApplicationName);
                        Description = Properties_Resources.YouCan;

                        Image slide = UIHelpers.GetImage ("tutorial-slide-4.png");

                        Button finish_button = new Button (Properties_Resources.Finish);
                        finish_button.Clicked += delegate {
                            Controller.TutorialPageCompleted ();
                        };


                        CheckButton check_button = new CheckButton (String.Format(Properties_Resources.Startup, Properties_Resources.ApplicationName)) {
                            Active = true
                        };

                        check_button.Toggled += delegate {
                            Controller.StartupItemChanged (check_button.Active);
                        };

                        Add (slide);
                        AddOption (check_button);
                        AddButton (finish_button);

                    }
                    break;
            }

        }

        public Setup () : base ()
        {
            Controller.HideWindowEvent += delegate {
                Application.Invoke (delegate {
                        HideAll ();
                        });
            };

            Controller.ShowWindowEvent += delegate {
                Application.Invoke (delegate {
                        ShowAll ();
                        Present ();
                        });
            };

            Controller.ChangePageEvent += delegate (PageType ptype) {
                Application.Invoke (delegate {
                        Reset ();

                        switch (ptype) {
                        case PageType.Setup:
                        ShowSetupPage();
                        break;

                        case PageType.Add1:
                        ShowAdd1Page();
                        break;

                        case PageType.Add2:
                        ShowAdd2Page();
                        break;

                        case PageType.Customize:
                        ShowCustomizePage();
                        break;

                        case PageType.Finished:
                        ShowFinishedPage();
                        break;

                        case PageType.Tutorial:
                        ShowTutorialPage();
                        break;
                        }

                        ShowAll ();
                });
            };
            this.DeleteEvent += delegate
            {
                Controller.PageCancelled();
            };
        }

        private void RenderServiceColumn (TreeViewColumn column, CellRenderer cell,
                TreeModel model, TreeIter iter)
        {
            string markup           = (string) model.GetValue (iter, 1);
            TreeSelection selection = (column.TreeView as TreeView).Selection;

            if (selection.IterIsSelected (iter)) {
                if (column.TreeView.HasFocus)
                    markup = markup.Replace (SecondaryTextColor, SecondaryTextColorSelected);
                else
                    markup = markup.Replace (SecondaryTextColorSelected, SecondaryTextColor);
            } else {
                markup = markup.Replace (SecondaryTextColorSelected, SecondaryTextColor);
            }

            (cell as CellRendererText).Markup = markup;
        }
    }

}
