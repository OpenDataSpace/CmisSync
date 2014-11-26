//-----------------------------------------------------------------------
// <copyright file="CredentialsWidget.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Config;

    [System.ComponentModel.ToolboxItem(true)]
    public partial class CredentialsWidget : Gtk.Bin
    {
        public event EventHandler Changed;
        public string UserName {
            get {
                return this.usernameEntry.Text;
            }

            set {
                this.usernameEntry.Text = value;
            }
        }

        public string Address {
            get {
                return this.urlWidget.Url;
            }

            set {
                this.urlWidget.Url = value;
            }
        }

        public string Password {
            get {
                return this.passwordEntry.Text;
            }

            set {
                this.passwordEntry.Text = value;
            }
        }

        public CredentialsWidget()
        {
            this.Build();
            this.addressLabel.Text = Properties_Resources.CmisWebAddress;
            this.urlWidget.ValidationActivated = false;
            this.urlWidget.IsUrlEditable = false;
            this.urlWidget.Changed += (object sender, EventArgs e) => {
                if (this.Changed != null) {
                    this.Changed(this, e);
                }
            };
        }

        protected void OnPasswordChanged(object sender, EventArgs e)
        {
            var handler = this.Changed;
            if (handler != null) {
                this.Changed(this, e);
            }
        }

        protected void OnUrlWidgetChanged(object sender, EventArgs e)
        {
            var handler = this.Changed;
            if (handler != null) {
                this.Changed(this, e);
            }
        }
    }
}