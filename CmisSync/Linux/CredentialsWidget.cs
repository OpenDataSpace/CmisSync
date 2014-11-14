
namespace CmisSync
{
    using System;

    using CmisSync.Lib.Config;

    [System.ComponentModel.ToolboxItem(true)]
    public partial class CredentialsWidget : Gtk.Bin
    {
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
                return this.addressEntry.Text;
            }

            set {
                this.addressEntry.Text = value;
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
        }
    }
}