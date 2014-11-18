using CmisSync.Lib.Config;


namespace CmisSync
{
    using System;

    public partial class Setting : Gtk.Window
    {
        private SettingController Controller = new SettingController();
        public Setting() : 
                base(Gtk.WindowType.Toplevel)
        {
            this.Build();
            this.HideOnDelete();
            this.Title = Properties_Resources.EditTitle;
            this.Controller.ShowWindowEvent += () => {
                this.Refresh();
                this.ShowAll();
            };
            this.Controller.HideWindowEvent += () => {
                this.Hide();
            };
            this.DeleteEvent += (o, args) => {
                // Do not destroy the window, just hide it
                args.RetVal = true;
                this.Hide();
            };
            this.label1.Text = Properties_Resources.SettingProxy;
            this.label2.Text = Properties_Resources.Features;
            this.cancelButton.Label = Properties_Resources.DiscardChanges;
            this.saveButton.Label = Properties_Resources.SaveChanges;
            this.cancelButton.Clicked += (object sender, EventArgs e) => this.Controller.HideWindow();
            this.saveButton.Clicked += (object sender, EventArgs e) => {
                var config = ConfigManager.CurrentConfig;
                config.Proxy = this.proxyWidget.Settings;
                config.Notifications = this.notificationToggleButton.Active;
                config.Save();
                this.Controller.HideWindow();
            };
            this.proxyWidget.Changed += (object sender, EventArgs e) => {
                this.saveButton.Sensitive = this.proxyWidget.IsValid;
            };
        }

        private void Refresh() {
            var config = ConfigManager.CurrentConfig;
            this.proxyWidget.Settings = config.Proxy;
            this.notificationToggleButton.Active = config.Notifications;
            this.saveButton.Sensitive = false;
        }

        protected void OnNotificationToggleButtonToggled(object sender, EventArgs e)
        {
            this.saveButton.Sensitive = this.proxyWidget.IsValid;
        }
    }
}