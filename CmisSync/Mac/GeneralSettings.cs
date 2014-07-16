//-----------------------------------------------------------------------
// <copyright file="GeneralSettings.cs" company="GRAU DATA AG">
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
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using CmisSync.Lib.Config;

namespace CmisSync
{
    public partial class GeneralSettings : MonoMac.AppKit.NSWindow
    {
        CmisSync.Lib.Config.ProxySettings ProxySettings
        {
            get;
            set;
        }

        #region Constructors

        // Called when created from unmanaged code
        public GeneralSettings(IntPtr handle) : base(handle)
        {
            Initialize();
        }
        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public GeneralSettings(NSCoder coder) : base(coder)
        {
            Initialize();
        }
        // Shared initialization code
        void Initialize()
        {
            this.Delegate = new SettingsDelegate();
        }

        #endregion

        SettingController Controller;

        public override void AwakeFromNib ()
        {
            base.AwakeFromNib ();
            this.Title = Properties_Resources.EditTitle;
            this.ProxySettings = ConfigManager.CurrentConfig.Proxy;
            this.CancelButton.Title = Properties_Resources.DiscardChanges;
            this.SaveButton.Title = Properties_Resources.SaveChanges;
            this.RequiresAuthorizationCheckBox.StringValue = Properties_Resources.NetworkProxyLogin;
            this.ProxyPasswordLabel.StringValue = Properties_Resources.Password;
            this.ProxyUsernameLabel.StringValue = Properties_Resources.User;

            Controller = (this.WindowController as GeneralSettingsController).Controller;

            this.ProxyServer.Delegate = new TextFieldDelegate ();
            (this.ProxyServer.Delegate as TextFieldDelegate).StringValueChanged += delegate
            {
                Controller.ValidateServer(this.ProxyServer.StringValue);
            };


            Controller.CheckProxyNoneEvent += (check) =>
            {
                if(check != (NoProxyButton.State == NSCellStateValue.On))
                {
                    NoProxyButton.State = check ? NSCellStateValue.On : NSCellStateValue.Off;
                }
            };

            Controller.CheckProxySystemEvent += (check) =>
            {
                if(check != (SystemDefaultProxyButton.State == NSCellStateValue.On))
                {
                    SystemDefaultProxyButton.State = check ? NSCellStateValue.On : NSCellStateValue.Off;
                }
            };

            Controller.CheckProxyCutomEvent += (check) =>
            {
                if(check != (ManualProxyButton.State == NSCellStateValue.On))
                {
                    ManualProxyButton.State = check ? NSCellStateValue.On : NSCellStateValue.Off;
                }
                ProxyServerLabel.Enabled = check;
                ProxyServer.Enabled = check;
                ProxyServerHelp.Enabled = check;
                CheckAddress(Controller);
            };

            Controller.EnableLoginEvent += (enable) =>
            {
                RequiresAuthorizationCheckBox.Enabled = enable;
                if (enable)
                {
                    Controller.CheckLogin(RequiresAuthorizationCheckBox.State == NSCellStateValue.On);
                }
                else
                {
                    ProxyUsernameLabel.Enabled = false;
                    ProxyUsername.Enabled = false;
                    ProxyPasswordLabel.Enabled = false;
                    ProxyPassword.Enabled = false;
                }
            };
            
            Controller.CheckLoginEvent += (check) =>
            {
                if (check != (RequiresAuthorizationCheckBox.State == NSCellStateValue.On))
                {
                    RequiresAuthorizationCheckBox.State = check ? NSCellStateValue.On : NSCellStateValue.Off;
                }
                ProxyUsernameLabel.Enabled = check;
                ProxyUsername.Enabled = check;
                ProxyPasswordLabel.Enabled = check;
                ProxyPassword.Enabled = check;
            };

            Controller.UpdateServerHelpEvent += (message) =>
            {
                ProxyServerHelp.StringValue = message;
            };


            Controller.UpdateSaveEvent += (enable) =>
            {
                SaveButton.Enabled = enable;
            };

            RefreshStates();
        }

        private void CheckAddress(SettingController controller)
        {
            if (ProxyServer.Enabled)
            {
                controller.ValidateServer(ProxyServer.StringValue);
            }
            else
            {
                SaveButton.Enabled = true;
                ProxyServerHelp.StringValue = String.Empty;
            }
        }

        public override void OrderFrontRegardless ()
        {
            NSApplication.SharedApplication.AddWindowsItem (this, Properties_Resources.ApplicationName, false);
            NSApplication.SharedApplication.ActivateIgnoringOtherApps (true);
            MakeKeyAndOrderFront (this);

            if (Program.UI != null)
                Program.UI.UpdateDockIconVisibility ();

            base.OrderFrontRegardless ();
            RefreshStates();
        }

        public override void PerformClose (NSObject sender)
        {
            base.OrderOut (this);
            NSApplication.SharedApplication.RemoveWindowsItem (this);

            if (Program.UI != null)
                Program.UI.UpdateDockIconVisibility ();

            return;
        }

        public class SettingsDelegate : NSWindowDelegate {

            public override bool WindowShouldClose (NSObject sender)
            {
                (sender as GeneralSettings).PerformClose(sender);
                return false;
            }
        }

        partial void OnSave(NSObject sender)
        {
            CmisSync.Lib.Config.ProxySettings settings = this.ProxySettings;
            if (NoProxyButton.State == NSCellStateValue.On) {
                settings.Selection = ProxySelection.NOPROXY;
            } else if(SystemDefaultProxyButton.State == NSCellStateValue.On) {
                settings.Selection = ProxySelection.SYSTEM;
            } else if(ManualProxyButton.State == NSCellStateValue.On) {
                settings.Selection = ProxySelection.CUSTOM;
            }
            string server = Controller.GetServer(this.ProxyServer.StringValue);
            if (server!=null)
            {
                settings.Server = new Uri(server);
            }
            settings.LoginRequired = (this.RequiresAuthorizationCheckBox.State == NSCellStateValue.On);
            settings.Username = this.ProxyUsername.StringValue;
            settings.ObfuscatedPassword = Crypto.Obfuscate(this.ProxyPassword.StringValue);
            this.ProxySettings = settings;
            ConfigManager.CurrentConfig.Proxy = settings;
            ConfigManager.CurrentConfig.Save();
            PerformClose(this);
        }

        partial void OnHelp(NSObject sender)
        {
            NSHelpManager.SharedHelpManager().FindString("proxy","MacHelp");
        }

        partial void OnCancel(NSObject sender)
        {
            PerformClose(this);
        }

        partial void OnRequireAuth(NSObject sender)
        {
            Controller.CheckLogin(this.RequiresAuthorizationCheckBox.State == NSCellStateValue.On);
        }

        partial void OnNoProxy(NSObject sender)
        {
            if(this.NoProxyButton.State == NSCellStateValue.On)
            {
                Controller.CheckProxyNone();
            }
        }

        partial void OnDefaultProxy(NSObject sender)
        {
            if(this.SystemDefaultProxyButton.State == NSCellStateValue.On)
            {
                Controller.CheckProxySystem();
            }
        }

        partial void OnManualProxy(NSObject sender)
        {
            if(this.ManualProxyButton.State == NSCellStateValue.On)
            {
                Controller.CheckProxyCustom();
            }
        }

        void RefreshStates()
        {
            Uri url = ProxySettings.Server;
            this.ProxyServer.StringValue = (url != null) ? url.ToString() : String.Empty;
            this.ProxyUsername.StringValue = (ProxySettings.Username != null) ? ProxySettings.Username : String.Empty;
            this.ProxyPassword.StringValue = (ProxySettings.ObfuscatedPassword != null) ? Crypto.Deobfuscate(ProxySettings.ObfuscatedPassword) : String.Empty;
            Controller.CheckLogin (ProxySettings.LoginRequired);
            if (ProxySettings.Selection == ProxySelection.NOPROXY) {
                Controller.CheckProxyNone ();
            } else if (ProxySettings.Selection == ProxySelection.SYSTEM) {
                Controller.CheckProxySystem ();
            } else if (ProxySettings.Selection == ProxySelection.CUSTOM) {
                Controller.CheckProxyCustom ();
            } else {
                Controller.CheckProxyNone ();
            }
        }
    }
}

