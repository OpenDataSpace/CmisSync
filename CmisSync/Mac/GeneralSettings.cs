using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using CmisSync.Lib;

namespace CmisSync
{
    public partial class GeneralSettings : MonoMac.AppKit.NSWindow
    {
        Config.ProxySettings ProxySettings
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
            Uri url = ProxySettings.Server;
            this.ProxyServer.StringValue = (url != null) ? url.ToString() : String.Empty;
            this.ProxyUsername.StringValue = (ProxySettings.Username != null) ? ProxySettings.Username : String.Empty;
            this.ProxyPassword.StringValue = (ProxySettings.ObfuscatedPassword != null) ? Crypto.Deobfuscate(ProxySettings.ObfuscatedPassword) : String.Empty;
            RefreshStates();
        }

        public override void OrderFrontRegardless ()
        {
            NSApplication.SharedApplication.AddWindowsItem (this, Properties_Resources.ApplicationName, false);
            NSApplication.SharedApplication.ActivateIgnoringOtherApps (true);
            MakeKeyAndOrderFront (this);

            if (Program.UI != null)
                Program.UI.UpdateDockIconVisibility ();

            base.OrderFrontRegardless ();
            this.ProxySettings = ConfigManager.CurrentConfig.Proxy;
            if (ProxySettings.Server != null)
            {
                Uri url = ProxySettings.Server;
                this.ProxyServer.StringValue = url.ToString();
            }
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
            Config.ProxySettings settings = this.ProxySettings;
            settings.Username = this.ProxyUsername.StringValue;
            settings.ObfuscatedPassword = Crypto.Obfuscate(this.ProxyPassword.StringValue);
            try{
                settings.Server = new Uri(this.ProxyServer.StringValue);
                ConfigManager.CurrentConfig.Proxy = settings;
                ConfigManager.CurrentConfig.Save();
                PerformClose(this);
            }
            catch(UriFormatException)
            {
                try{
                    if(!this.ProxyServer.StringValue.StartsWith("http://"))
                    {
                        settings.Server = new Uri("http://" + this.ProxyServer.StringValue);
                        ConfigManager.CurrentConfig.Proxy = settings;
                        ConfigManager.CurrentConfig.Save();
                        PerformClose(this);
                    }
                }catch(UriFormatException){}
            }
        }

        partial void OnHelp(NSObject sender)
        {
            throw new System.NotImplementedException();
        }

        partial void OnCancel(NSObject sender)
        {
            PerformClose(this);
        }

        partial void OnRequireAuth(NSObject sender)
        {
            Config.ProxySettings settings = this.ProxySettings;
            settings.LoginRequired = (this.RequiresAuthorizationCheckBox.State == NSCellStateValue.On);
            this.ProxySettings = settings;
            RefreshStates();
        }

        partial void OnNoProxy(NSObject sender)
        {
            if(this.NoProxyButton.State == NSCellStateValue.On)
            {
                Config.ProxySettings settings = this.ProxySettings;
                settings.Selection = Config.ProxySelection.NOPROXY;
                settings.LoginRequired = false;
                this.ProxySettings = settings;
            }
            RefreshStates();
        }

        partial void OnDefaultProxy(NSObject sender)
        {
            if(this.SystemDefaultProxyButton.State == NSCellStateValue.On)
            {
                Config.ProxySettings settings = ProxySettings;
                settings.Selection = Config.ProxySelection.SYSTEM;
                ProxySettings = settings;
            }
            RefreshStates();
        }

        partial void OnManualProxy(NSObject sender)
        {
            if(this.ManualProxyButton.State == NSCellStateValue.On)
            {
                Config.ProxySettings settings = ProxySettings;
                settings.Selection = Config.ProxySelection.CUSTOM;
                ProxySettings = settings;
            }
            RefreshStates();
        }

        void RefreshStates()
        {
            this.RequiresAuthorizationCheckBox.State = ProxySettings.LoginRequired ? NSCellStateValue.On : NSCellStateValue.Off;
            this.ProxyUsername.Enabled = ProxySettings.LoginRequired;
            this.ProxyPassword.Enabled = ProxySettings.LoginRequired;
            this.ProxyUsernameLabel.Enabled = ProxySettings.LoginRequired;
            this.ProxyPasswordLabel.Enabled = ProxySettings.LoginRequired;
            this.ProxyServerLabel.Enabled = ProxySettings.Selection == Config.ProxySelection.CUSTOM;
            this.ProxyServer.Enabled = ProxySettings.Selection == Config.ProxySelection.CUSTOM;
            this.RequiresAuthorizationCheckBox.Enabled = ProxySettings.Selection != Config.ProxySelection.NOPROXY;
            this.NoProxyButton.State = ProxySettings.Selection == Config.ProxySelection.NOPROXY ? NSCellStateValue.On : NSCellStateValue.Off;
            this.SystemDefaultProxyButton.State = ProxySettings.Selection == Config.ProxySelection.SYSTEM ? NSCellStateValue.On : NSCellStateValue.Off;
            this.ManualProxyButton.State = ProxySettings.Selection == Config.ProxySelection.CUSTOM ? NSCellStateValue.On : NSCellStateValue.Off;
        }
    }
}

