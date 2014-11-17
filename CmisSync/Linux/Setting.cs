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
            this.Controller.HideWindowEvent += () => this.Hide();
            this.label1.Text = Properties_Resources.SettingProxy;
            this.label2.Text = Properties_Resources.Features;
            this.cancelButton.Label = Properties_Resources.DiscardChanges;
            this.saveButton.Label = Properties_Resources.SaveChanges;
            this.cancelButton.Activated += (object sender, EventArgs e) => this.Controller.HideWindow();
            this.saveButton.Activated += (object sender, EventArgs e) => this.Controller.HideWindow();
        }

        private void Refresh() {
            this.saveButton.Sensitive = false;
            var config = ConfigManager.CurrentConfig;
            this.proxyWidget.Settings = config.Proxy;
            this.notificationToggleButton.Active = config.Notifications;
        }
    }
}