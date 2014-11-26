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
        public string FolderName { get; set; }

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
            this.FolderName = name;
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
            this.Header = CmisSync.Properties_Resources.EditTitle;

            VBox layout_vertical   = new VBox(false, 12);

            Button cancel_button = new Button(CmisSync.Properties_Resources.Cancel);
            cancel_button.Clicked += delegate {
                this.Close();
            };

            Button finish_button = new Button(CmisSync.Properties_Resources.SaveChanges);
            finish_button.Sensitive = false;
            finish_button.Clicked += delegate {
                this.Controller.SaveFolder();
                this.Close();
            };

            layout_vertical.PackStart(new Label(string.Empty), false, false, 0);
            Notebook tab_view = new Notebook();
            //tab_view.AppendPage(layout_vertical, new Label("Edit Folders"));
            var credentialsWidget = new CredentialsWidget();
            credentialsWidget.Password = this.Credentials.Password.ToString();
            credentialsWidget.Address = this.Credentials.Address.ToString();
            credentialsWidget.UserName = this.Credentials.UserName;
            credentialsWidget.Changed += (object sender, EventArgs e) => finish_button.Sensitive = true;
            tab_view.AppendPage(credentialsWidget, new Label(Properties_Resources.Credentials));
            this.Add(tab_view);
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
                return this.Visible;
            }

            private set
            {
                this.Visible = value;
            }
        }
    }
}
