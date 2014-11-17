
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
            this.urlWidget.Title = Properties_Resources.CmisWebAddress;
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