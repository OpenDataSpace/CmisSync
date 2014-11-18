
namespace CmisSync.Widgets
{
    using System;

    using CmisSync.Lib.Config;

    [System.ComponentModel.ToolboxItem(true)]
    public partial class ProxyWidget : Gtk.Bin
    {
        private ProxySettings settings;

        public event EventHandler Changed;
        public ProxyWidget()
        {
            this.Build();
            this.IsValid = true;
        }

        public ProxySettings ProxySettings {
            get {
                return this.settings;
            }

            set {
                this.credentialsRequiredButton.Active = value.LoginRequired;
                this.Selection = value.Selection;
                this.urlWidget.Url = value.Server != null ? value.Server.ToString() : string.Empty;
                this.urlWidget.ValidationActivated = true;
                this.userEntry.Text = value.Username ?? string.Empty;
                this.passwordEntry.Text = new Password() { ObfuscatedPassword = value.ObfuscatedPassword }.ToString() ?? string.Empty;
            }
        }

        public bool IsValid { get; private set; }

        public ProxySelection Selection { get {
                return this.settings.Selection;
            }

            set {
                this.settings.Selection = value;
                switch (value) {
                case ProxySelection.NOPROXY:
                    this.passwordEntry.Sensitive = false;
                    this.userEntry.Sensitive = false;
                    this.urlWidget.IsUrlEditable = false;
                    this.credentialsRequiredButton.Sensitive = false;
                    this.noProxyButton.Active = true;
                    this.IsValid = true;
                    break;
                case ProxySelection.SYSTEM:
                    this.passwordEntry.Sensitive = false;
                    this.userEntry.Sensitive = false;
                    this.urlWidget.IsUrlEditable = false;
                    this.credentialsRequiredButton.Sensitive = false;
                    this.systemProxyButton.Active = true;
                    this.IsValid = true;
                    break;
                case ProxySelection.CUSTOM:
                    this.urlWidget.IsUrlEditable = true;
                    this.credentialsRequiredButton.Sensitive = true;
                    this.passwordEntry.Sensitive = this.credentialsRequiredButton.Active;
                    this.userEntry.Sensitive = this.credentialsRequiredButton.Active;
                    this.customProxyButton.Active = true;
                    break;
                }
            }
        }

        protected void OnNoProxyButtonActivated(object sender, EventArgs e)
        {
            if (this.noProxyButton.Active) {
                this.Selection = ProxySelection.NOPROXY;
            }

            this.OnChange(sender, e);
        }

        protected void OnSystemProxyButtonActivated(object sender, EventArgs e)
        {
            if (this.systemProxyButton.Active) {
                this.Selection = ProxySelection.SYSTEM;
            }

            this.OnChange(sender, e);
        }

        protected void OnCustomProxyButtonActivated(object sender, EventArgs e)
        {
            if (this.customProxyButton.Active) {
                this.Selection = ProxySelection.CUSTOM;
            }

            this.OnChange(sender, e);
        }

        protected void OnCredentialsRequiredButtonClicked(object sender, EventArgs e)
        {
            this.settings.LoginRequired = this.credentialsRequiredButton.Active;
            this.userEntry.Sensitive = this.credentialsRequiredButton.Active;
            this.passwordEntry.Sensitive = this.credentialsRequiredButton.Active;
            this.OnChange(sender, e);
        }

        protected void OnChange(object sender, EventArgs e) {
            this.settings.Username = this.userEntry.Text;
            this.settings.ObfuscatedPassword = new Password(this.passwordEntry.Text).ObfuscatedPassword;
            var handler = this.Changed;
            if (handler != null) {
                handler(this, e);
            }
        }

        protected void OnUrlWidgetChanged(object sender, EventArgs e)
        {
            this.IsValid = this.urlWidget.IsValidUrl;
            if (this.IsValid) {
                try {
                    this.settings.Server = new Uri(this.urlWidget.Url);
                    this.OnChange(sender, e);
                } catch (Exception) {
                    this.IsValid = false;
                }
            }
        }
    }
}