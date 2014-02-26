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
    public class Setting : Window
    {
        private SettingController Controller = new SettingController();

        public Setting()
        {
            Title = Properties_Resources.EditTitle;
            ResizeMode = ResizeMode.NoResize;
            Height = 288;
            Width = 640;
            Icon = UIHelpers.GetImageSource("app", "ico");

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            Closing += delegate (object sender, CancelEventArgs args)
            {
                Controller.HideWindow();
                args.Cancel = true;
            };

            CreateSetting();

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
        }

        private RadioButton ProxyNone = new RadioButton();
        private RadioButton ProxySystem = new RadioButton();
        private RadioButton ProxyCustom = new RadioButton();
        private CheckBox LoginCheck = new CheckBox();
        private TextBlock AddressLabel;
        private TextBox AddressText;
        private TextBlock UserLabel;
        private TextBox UserText;
        private TextBlock PasswordLabel;
        private PasswordBox PasswordText;
        Button FinishButton = new Button();
        Button CancelButton = new Button();

        private void SelectProxyCustom(bool select)
        {
            ProxyCustom.IsChecked = select;
            AddressText.IsEnabled = select;
            LoginCheck.IsEnabled = select;
            UpdateProxyLogin();
        }

        private void UpdateProxyLogin()
        {
            if (LoginCheck.IsEnabled && LoginCheck.IsChecked.GetValueOrDefault())
            {
                UserText.IsEnabled = true;
                PasswordText.IsEnabled = true;
            }
            else
            {
                UserText.IsEnabled = false;
                PasswordText.IsEnabled = false;
            }
        }

        private void RefreshSetting()
        {
            LoginCheck.IsChecked = ConfigManager.CurrentConfig.Proxy.LoginRequired;
            AddressText.Text = ConfigManager.CurrentConfig.Proxy.Server == null ? "" : ((Uri)ConfigManager.CurrentConfig.Proxy.Server).ToString();
            UserText.Text = ConfigManager.CurrentConfig.Proxy.Username == null ? "" : ConfigManager.CurrentConfig.Proxy.Username;
            PasswordText.Password = ConfigManager.CurrentConfig.Proxy.ObfuscatedPassword == null ? "" : Crypto.Deobfuscate(ConfigManager.CurrentConfig.Proxy.ObfuscatedPassword);
            SelectProxyCustom(false);
            switch (ConfigManager.CurrentConfig.Proxy.Selection)
            {
                case Config.ProxySelection.NOPROXY:
                    SelectProxyCustom(true);
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

        private void CreateSetting()
        {
            int textFullLength = 500;
            int textMiddleLength = 240;
            int radioLeft = 50;
            int textFullLeft = 70;
            int textMiddleLeft = textFullLeft + textFullLength - textMiddleLength;

            ProxyNone.GroupName = ProxySystem.GroupName = ProxyCustom.GroupName = "proxy";
            ProxyNone.Content = Properties_Resources.NetworkProxySelectNone;
            ProxySystem.Content = Properties_Resources.NetworkProxySelectSystem;
            ProxyCustom.Content = Properties_Resources.NetworkProxySelectCustom;
            ProxyCustom.Checked += delegate
            {
                SelectProxyCustom(true);
            };
            ProxyCustom.Unchecked += delegate
            {
                SelectProxyCustom(false);
            };

            LoginCheck.Content = Properties_Resources.NetworkProxyLogin;
            LoginCheck.Checked += delegate
            {
                UpdateProxyLogin();
            };
            LoginCheck.Unchecked += delegate
            {
                UpdateProxyLogin();
            };

            AddressLabel = new TextBlock()
            {
                Width = textFullLength,
                Text = Properties_Resources.NetworkProxyServer + ":",
                FontWeight = FontWeights.Bold
            };

            AddressText = new TextBox()
            {
                Width = textFullLength,
                Text = ""
            };

            UserLabel = new TextBlock()
            {
                Width = textMiddleLength,
                Text = Properties_Resources.User + ":",
                FontWeight = FontWeights.Bold,
            };

            UserText = new TextBox()
            {
                Width = textMiddleLength,
                Text = ""
            };

            PasswordLabel = new TextBlock()
            {
                Width = textMiddleLength,
                Text = Properties_Resources.Password + ":",
                FontWeight = FontWeights.Bold,
            };

            PasswordText = new PasswordBox()
            {
                Width = textMiddleLength,
                Password = ""
            };

            FinishButton = new Button()
            {
                Content = Properties_Resources.SaveChanges,
                IsDefault = true
            };

            CancelButton = new Button()
            {
                Content = Properties_Resources.DiscardChanges,
                IsDefault = false
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

            Canvas canvas = new Canvas();

            canvas.Children.Add(ProxyNone);
            Canvas.SetLeft(ProxyNone, radioLeft);
            Canvas.SetTop(ProxyNone, 30);

            canvas.Children.Add(ProxySystem);
            Canvas.SetLeft(ProxySystem, radioLeft);
            Canvas.SetTop(ProxySystem, 50);

            canvas.Children.Add(ProxyCustom);
            Canvas.SetLeft(ProxyCustom, radioLeft);
            Canvas.SetTop(ProxyCustom, 70);

            canvas.Children.Add(AddressLabel);
            Canvas.SetLeft(AddressLabel, textFullLeft);
            Canvas.SetTop(AddressLabel, 90);

            canvas.Children.Add(AddressText);
            Canvas.SetLeft(AddressText, textFullLeft);
            Canvas.SetTop(AddressText, 110);

            canvas.Children.Add(LoginCheck);
            Canvas.SetLeft(LoginCheck, textFullLeft);
            Canvas.SetTop(LoginCheck, 140);

            canvas.Children.Add(UserLabel);
            Canvas.SetLeft(UserLabel, textFullLeft);
            Canvas.SetTop(UserLabel, 160);

            canvas.Children.Add(UserText);
            Canvas.SetLeft(UserText, textFullLeft);
            Canvas.SetTop(UserText, 180);

            canvas.Children.Add(PasswordLabel);
            Canvas.SetLeft(PasswordLabel, textMiddleLeft);
            Canvas.SetTop(PasswordLabel, 160);

            canvas.Children.Add(PasswordText);
            Canvas.SetLeft(PasswordText, textMiddleLeft);
            Canvas.SetTop(PasswordText, 180);

            canvas.Children.Add(CancelButton);
            Canvas.SetLeft(CancelButton, textMiddleLeft);
            Canvas.SetTop(CancelButton, 220);

            canvas.Children.Add(FinishButton);
            Canvas.SetLeft(FinishButton, textMiddleLeft + 100);
            Canvas.SetTop(FinishButton, 220);

            Content = canvas;
        }

    }
}
