using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

using CmisSync.Lib;


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
                Config.ProxySettings proxy = new Config.ProxySettings();
                if (ProxyNone.IsChecked.GetValueOrDefault())
                {
                    proxy.Selection = Config.ProxySelection.NOPROXY;
                }
                else if (ProxySystem.IsChecked.GetValueOrDefault())
                {
                    proxy.Selection = Config.ProxySelection.SYSTEM;
                }
                else
                {
                    proxy.Selection = Config.ProxySelection.CUSTOM;
                }
                proxy.LoginRequired = LoginCheck.IsChecked.GetValueOrDefault();
                proxy.Server = new Uri(AddressText.Text);
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
        Button FinishButton;
        Button CancelButton;

        private void RefreshSetting()
        {
            AddressText.Text = ConfigManager.CurrentConfig.Proxy.Server == null ? "" : ((Uri)ConfigManager.CurrentConfig.Proxy.Server).ToString();
            UserText.Text = ConfigManager.CurrentConfig.Proxy.Username == null ? "" : ConfigManager.CurrentConfig.Proxy.Username;
            PasswordText.Password = ConfigManager.CurrentConfig.Proxy.ObfuscatedPassword == null ? "" : Crypto.Deobfuscate(ConfigManager.CurrentConfig.Proxy.ObfuscatedPassword);

            LoginCheck.IsChecked = ConfigManager.CurrentConfig.Proxy.LoginRequired;

            //  Force to trigger Checked and Unchecked event handle
            ProxyNone.IsChecked = true;
            ProxySystem.IsChecked = true;
            ProxyCustom.IsChecked = true;

            switch (ConfigManager.CurrentConfig.Proxy.Selection)
            {
                case Config.ProxySelection.NOPROXY:
                    ProxyNone.IsChecked = true;
                    break;
                case Config.ProxySelection.SYSTEM:
                    ProxySystem.IsChecked = true;
                    break;
                case Config.ProxySelection.CUSTOM:
                    ProxyCustom.IsChecked = true;
                    break;
                default:
                    break;
            }

            FinishButton.Focus();
        }

        private void LoadSetting()
        {
            System.Uri resourceLocater = new System.Uri("/DataSpaceSync;component/SettingWPF.xaml", System.UriKind.Relative);
            UserControl SettingWPF = Application.LoadComponent(resourceLocater) as UserControl;

            ProxyNone = SettingWPF.FindName("ProxyNone") as RadioButton;
            ProxySystem = SettingWPF.FindName("ProxySystem") as RadioButton;
            ProxyCustom = SettingWPF.FindName("ProxyCustom") as RadioButton;
            LoginCheck = SettingWPF.FindName("LoginCheck") as CheckBox;
            AddressLabel = SettingWPF.FindName("AddressLabel") as TextBlock;
            AddressText = SettingWPF.FindName("AddressText") as TextBox;
            UserLabel = SettingWPF.FindName("UserLabel") as TextBlock;
            UserText = SettingWPF.FindName("UserText") as TextBox;
            PasswordLabel = SettingWPF.FindName("PasswordLabel") as TextBlock;
            PasswordText = SettingWPF.FindName("PasswordText") as PasswordBox;
            FinishButton = SettingWPF.FindName("FinishButton") as Button;
            CancelButton = SettingWPF.FindName("CancelButton") as Button;

            Content = SettingWPF;
        }

    }
}
