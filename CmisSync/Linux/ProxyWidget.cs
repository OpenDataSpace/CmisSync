using CmisSync.Lib.Config;


namespace CmisSync.Widgets
{
    using System;
    [System.ComponentModel.ToolboxItem(true)]
    public partial class ProxyWidget : Gtk.Bin
    {
        private ProxySettings settings;
        public ProxyWidget()
        {
            this.Build();
        }

        public ProxySettings Settings {
            get {
                return this.settings;
            } set {
                this.Selection = value.Selection;
                this.urlWidget.Url = value.Server != null ? value.Server.ToString() : string.Empty;
                this.urlWidget.ValidationActivated = true;
            }
        }


        public ProxySelection Selection { get {
                return this.settings.Selection;
            }

            set {
                this.settings.Selection = value;
                switch (value) {
                case ProxySelection.NOPROXY:
                    goto case ProxySelection.SYSTEM;
                case ProxySelection.SYSTEM:
                    this.passwordEntry.Sensitive = false;
                    this.userEntry.Sensitive = false;
                    this.urlWidget.IsUrlEditable = false;
                    break;
                case ProxySelection.CUSTOM:
                    this.urlWidget.IsUrlEditable = true;
                    break;
                }
            }
        }

        protected void OnSystemProxyButtonActivated(object sender, EventArgs e)
        {
            this.Selection = ProxySelection.SYSTEM;
        }

        protected void OnNoProxyButtonActivated(object sender, EventArgs e)
        {
            this.Selection = ProxySelection.NOPROXY;
        }

        protected void OnCustomProxyButtonActivated(object sender, EventArgs e)
        {
            this.Selection = ProxySelection.CUSTOM;
        }
    }
}