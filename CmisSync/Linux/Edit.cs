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

namespace CmisSync
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    using CmisSync.CmisTree;
    using CmisSync.Lib.Config;

    using Gtk;

    /// <summary>
    /// Edit folder diaglog
    /// It allows user to edit the selected and ignored folders
    /// </summary>
    [CLSCompliant(false)]
    public class Edit : SetupWindow
    {
        /// <summary>
        /// Controller
        /// </summary>
        public EditController Controller = new EditController();

        /// <summary>
        /// Synchronized folder name
        /// </summary>
        public string Name { get; set; }

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
            EditCredentials,
        };

        private EditType type;

        /// <summary>
        /// Constructor
        /// </summary>
        public Edit(EditType type, CmisRepoCredentials credentials, string name, string remotePath, List<string> ignores, string localPath)
        {
            this.Name = name;
            this.Credentials = credentials;
            this.remotePath = remotePath;
            this.Ignores = ignores;
            this.localPath = localPath;
            this.type = type;

            this.CreateEdit();

            this.Deletable = true;

            this.DeleteEvent += delegate(object sender, DeleteEventArgs args) {
                args.RetVal = false;
                this.Controller.CloseWindow();
            };

            this.Controller.OpenWindowEvent += delegate
            {
                this.ShowAll();
                this.Activate();
            };
        }

        /// <summary>
        /// Create the UI
        /// </summary>
        private void CreateEdit()
        {
            CmisTreeStore cmisStore = new CmisTreeStore();
            Gtk.TreeView treeView = new Gtk.TreeView(cmisStore);

            RootFolder root = new RootFolder() {
                Name = this.Name,
                Id = this.Credentials.RepoId,
                Address = this.Credentials.Address.ToString()
            };
            IgnoredFolderLoader.AddIgnoredFolderToRootNode(root, this.Ignores);
            LocalFolderLoader.AddLocalFolderToRootNode(root, this.localPath);

            AsyncNodeLoader asyncLoader = new AsyncNodeLoader(root, this.Credentials, PredefinedNodeLoader.LoadSubFolderDelegate, PredefinedNodeLoader.CheckSubFolderDelegate);
            asyncLoader.UpdateNodeEvent += delegate {
                cmisStore.UpdateCmisTree(root);
            };
            cmisStore.UpdateCmisTree(root);
            asyncLoader.Load(root);

            this.Header = CmisSync.Properties_Resources.EditTitle;

            VBox layout_vertical   = new VBox(false, 12);

            this.Controller.CloseWindowEvent += delegate
            {
                asyncLoader.Cancel();
            };

            Button cancel_button = new Button(CmisSync.Properties_Resources.Cancel);
            cancel_button.Clicked += delegate {
                this.Close();
            };

            Button finish_button = new Button(CmisSync.Properties_Resources.SaveChanges);
            finish_button.Clicked += delegate {
                this.Ignores = NodeModelUtils.GetIgnoredFolder(root);
                this.Controller.SaveFolder();
                this.Close();
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
                if (!cmisStore.GetIterFromString(out iterToggled, args.Path))
                {
                    Console.WriteLine("Toggled GetIter Error " + args.Path);
                    return;
                }

                Node node = cmisStore.GetValue(iterToggled, (int)CmisTreeStore.Column.ColumnNode) as Node;
                if (node == null)
                {
                    Console.WriteLine("Toggled GetValue Error " + args.Path);
                    return;
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
            CellRendererText renderText = new CellRendererText();
            column.PackStart(renderText, false);
            column.SetAttributes(renderText, "text", (int)CmisTreeStore.Column.ColumnName);
            column.Expand = true;
            treeView.AppendColumn(column);

            treeView.AppendColumn("Status", new StatusCellRenderer(), "text", (int)CmisTreeStore.Column.ColumnStatus);

            treeView.RowExpanded += delegate(object o, RowExpandedArgs args) {
                Node node = cmisStore.GetValue(args.Iter, (int)CmisTreeStore.Column.ColumnNode) as Node;
                asyncLoader.Load(node);
            };

            ScrolledWindow sw = new ScrolledWindow() {
                ShadowType = Gtk.ShadowType.In
            };
            sw.Add(treeView);

            layout_vertical.PackStart(new Label(string.Empty), false, false, 0);
            layout_vertical.PackStart(sw, true, true, 0);
            this.Add(layout_vertical);
            this.AddButton(cancel_button);
            this.AddButton(finish_button);

            finish_button.GrabDefault();

            this.ShowAll();
        }

        /// <summary>
        /// Close the UI
        /// </summary>
        public void Close()
        {
            this.Controller.CloseWindow();
            this.Destroy();
        }

        /// <summary>
        /// Gets a value indicating whether this window is visible.
        /// TODO Should be implemented with the correct Windows property,
        /// at the moment, it always returns false
        /// </summary>
        /// <value>
        /// <c>true</c> if this window is visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsVisible {
            get {
                // TODO Please change it to the correct Window property if this method is needed
                return false;
            }

            private set
            {
            }
        }
    }
}
