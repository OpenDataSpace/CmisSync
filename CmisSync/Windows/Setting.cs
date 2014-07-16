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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Forms.Integration;

using CmisSync.Lib.Config;


namespace CmisSync
{
    /// <summary>
    /// Setting widget
    /// </summary>
    public class Setting : Window
    {
        private SettingController Controller = new SettingController();

        /// <summary>
        /// Constructor
        /// </summary>
        public Setting()
        {
            Title = Properties_Resources.EditTitle;
            ResizeMode = ResizeMode.NoResize;
            Height = 340;
            Width = 640;
            Icon = UIHelpers.GetImageSource("app", "ico");
            ElementHost.EnableModelessKeyboardInterop(this);

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            Closing += delegate (object sender, CancelEventArgs args)
            {
                Controller.HideWindow();
                args.Cancel = true;
            };

            LoadSetting();

            Controller.ShowWindowEvent += delegate
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    RefreshSetting();
                    Show();
                    Activate();
                    BringIntoView();
                });
            };

            Controller.HideWindowEvent += delegate
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    Hide();
                });
            };

            FinishButton.Click += delegate
            {
                ProxySettings proxy = new ProxySettings();
                if (ProxyNone.IsChecked.GetValueOrDefault())
                {
                    proxy.Selection = ProxySelection.NOPROXY;
                }
                else if (ProxySystem.IsChecked.GetValueOrDefault())
                {
                    proxy.Selection = ProxySelection.SYSTEM;
                }
                else
                {
                    proxy.Selection = ProxySelection.CUSTOM;
                }
                proxy.LoginRequired = LoginCheck.IsChecked.GetValueOrDefault();
                string server = Controller.GetServer(AddressText.Text);
                if (server != null)
                {
                    proxy.Server = new Uri(server);
                }
                else
                {
                    proxy.Server = ConfigManager.CurrentConfig.Proxy.Server;
                }
                proxy.Username = UserText.Text;
                proxy.ObfuscatedPassword = Crypto.Obfuscate(PasswordText.Password);

                ConfigManager.CurrentConfig.Proxy = proxy;
                ConfigManager.CurrentConfig.Save();

                Controller.HideWindow();
            };

            CancelButton.Click += delegate
            {
                Controller.HideWindow();
            };
        }

        private RadioButton ProxyNone;
        private RadioButton ProxySystem;
        private RadioButton ProxyCustom;
        private CheckBox LoginCheck;
        private TextBlock AddressLabel;
        private TextBox AddressText;
        private TextBlock UserLabel;
        private TextBox UserText;
        private TextBlock PasswordLabel;
        private PasswordBox PasswordText;
        private Button FinishButton;
        private Button CancelButton;

        private void RefreshSetting()
        {
            AddressText.Text = ConfigManager.CurrentConfig.Proxy.Server == null ? "" : ((Uri)ConfigManager.CurrentConfig.Proxy.Server).ToString();
            UserText.Text = ConfigManager.CurrentConfig.Proxy.Username == null ? "" : ConfigManager.CurrentConfig.Proxy.Username;
            PasswordText.Password = ConfigManager.CurrentConfig.Proxy.ObfuscatedPassword == null ? "" : Crypto.Deobfuscate(ConfigManager.CurrentConfig.Proxy.ObfuscatedPassword);

            Controller.CheckLogin(ConfigManager.CurrentConfig.Proxy.LoginRequired);

            switch (ConfigManager.CurrentConfig.Proxy.Selection)
            {
                case ProxySelection.NOPROXY:
                    Controller.CheckProxyNone();
                    break;
                case ProxySelection.SYSTEM:
                    Controller.CheckProxySystem();
                    break;
                case ProxySelection.CUSTOM:
                    Controller.CheckProxyCustom();
                    break;
                default:
                    break;
            }

            FinishButton.Focus();
        }

        private void LoadSetting()
        {
            System.Uri resourceLocater = new System.Uri("/DataSpaceSync;component/SettingWPF.xaml", System.UriKind.Relative);
            SettingWPF wpf = Application.LoadComponent(resourceLocater) as SettingWPF;

            ProxyNone = wpf.FindName("ProxyNone") as RadioButton;
            ProxySystem = wpf.FindName("ProxySystem") as RadioButton;
            ProxyCustom = wpf.FindName("ProxyCustom") as RadioButton;
            LoginCheck = wpf.FindName("LoginCheck") as CheckBox;
            AddressLabel = wpf.FindName("AddressLabel") as TextBlock;
            AddressText = wpf.FindName("AddressText") as TextBox;
            UserLabel = wpf.FindName("UserLabel") as TextBlock;
            UserText = wpf.FindName("UserText") as TextBox;
            PasswordLabel = wpf.FindName("PasswordLabel") as TextBlock;
            PasswordText = wpf.FindName("PasswordText") as PasswordBox;
            FinishButton = wpf.FindName("FinishButton") as Button;
            CancelButton = wpf.FindName("CancelButton") as Button;

            wpf.ApplyController(Controller);

            Content = wpf;
        }

    }
}
