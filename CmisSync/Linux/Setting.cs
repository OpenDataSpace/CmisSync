//-----------------------------------------------------------------------
// <copyright file="Setting.cs" company="GRAU DATA AG">
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
using CmisSync.Lib.Config;


namespace CmisSync
{
    using System;

    using log4net;

    public partial class Setting : Gtk.Window
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Setting));

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
                config.Proxy = this.proxyWidget.ProxySettings;
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
            this.proxyWidget.ProxySettings = config.Proxy;
            this.notificationToggleButton.Active = config.Notifications;
            this.saveButton.Sensitive = false;
        }

        protected void OnNotificationToggleButtonToggled(object sender, EventArgs e)
        {
            Logger.Debug("Notification Settings toggled to " + (this.notificationToggleButton.Active ? "true" : "false"));
            this.saveButton.Sensitive = this.proxyWidget.IsValid;
        }
    }
}