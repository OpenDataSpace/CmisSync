//-----------------------------------------------------------------------
// <copyright file="SettingWPF.xaml.cs" company="GRAU DATA AG">
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CmisSync
{
    /// <summary>
    /// Interaction logic for SettingWPF.xaml
    /// </summary>
    public partial class SettingWPF : UserControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SettingWPF()
        {
            InitializeComponent();
            ApplySetting();
        }

        private void CheckAddress(SettingController controller)
        {
            if (AddressText.IsEnabled)
            {
                controller.ValidateServer(AddressText.Text);
            }
            else
            {
                FinishButton.IsEnabled = true;
                AddressError.Text = String.Empty;
            }
        }

        public void ApplyController(SettingController controller)
        {
            ProxyNone.Checked += delegate { controller.CheckProxyNone(); };
            ProxySystem.Checked += delegate { controller.CheckProxySystem(); };
            ProxyCustom.Checked += delegate { controller.CheckProxyCustom(); };
            LoginCheck.Checked += delegate { controller.CheckLogin(true); };
            LoginCheck.Unchecked += delegate { controller.CheckLogin(false); };
            AddressText.TextChanged += delegate(object sender, TextChangedEventArgs e)
            {
                controller.ValidateServer(AddressText.Text);
            };

            controller.CheckProxyNoneEvent += (check) =>
            {
                if (check != ProxyNone.IsChecked)
                {
                    ProxyNone.IsChecked = check;
                }
            };

            controller.CheckProxySystemEvent += (check) =>
            {
                if (check != ProxySystem.IsChecked)
                {
                    ProxySystem.IsChecked = check;
                }
            };

            controller.CheckProxyCutomEvent += (check) =>
            {
                if (check != ProxyCustom.IsChecked)
                {
                    ProxyCustom.IsChecked = check;
                }
                AddressText.IsEnabled = check;
                CheckAddress(controller);
            };

            controller.EnableLoginEvent += (enable) =>
            {
                LoginCheck.IsEnabled = enable;
                if (enable)
                {
                    controller.CheckLogin(LoginCheck.IsChecked.GetValueOrDefault());
                }
                else
                {
                    UserText.IsEnabled = false;
                    PasswordText.IsEnabled = false;
                }
            };

            controller.CheckLoginEvent += (check) =>
            {
                if (check != LoginCheck.IsChecked)
                {
                    LoginCheck.IsChecked = check;
                }
                UserText.IsEnabled = check;
                PasswordText.IsEnabled = check;
            };

            controller.UpdateServerHelpEvent += (message) =>
            {
                AddressError.Text = message;
            };

            controller.UpdateSaveEvent += (enable) =>
            {
                FinishButton.IsEnabled = enable;
            };

        }

        private void ApplySetting()
        {
            ProxyTab.Header = Properties_Resources.SettingProxy;
            ProxyNone.GroupName = ProxySystem.GroupName = ProxyCustom.GroupName = "proxy";
            ProxyNone.Content = Properties_Resources.NetworkProxySelectNone;
            ProxySystem.Content = Properties_Resources.NetworkProxySelectSystem;
            ProxyCustom.Content = Properties_Resources.NetworkProxySelectCustom;

            AddressLabel.Text = Properties_Resources.NetworkProxyServer + ":";

            LoginCheck.Content = Properties_Resources.NetworkProxyLogin;

            UserLabel.Text = Properties_Resources.User + ":";
            PasswordLabel.Text = Properties_Resources.Password + ":";

            FinishButton.Content = Properties_Resources.SaveChanges;
            CancelButton.Content = Properties_Resources.DiscardChanges;
        }
    }
}
