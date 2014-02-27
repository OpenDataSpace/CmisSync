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

        private void SelectProxy()
        {
            if (ProxyNone.IsChecked.GetValueOrDefault())
            {
                AddressText.IsEnabled = false;
                LoginCheck.IsEnabled = false;
            }
            else if (ProxySystem.IsChecked.GetValueOrDefault())
            {
                AddressText.IsEnabled = false;
                LoginCheck.IsEnabled = true;
            }
            else if (ProxyCustom.IsChecked.GetValueOrDefault())
            {
                AddressText.IsEnabled = true;
                LoginCheck.IsEnabled = true;
            }
            UpdateProxyLogin();
            CheckAddress();
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

        private void CheckAddress()
        {
            if (AddressText.IsEnabled)
            {
                string uriString = AddressText.Text;
                try
                {
                    Uri uri = new Uri(uriString);
                }
                catch (Exception)
                {
                    FinishButton.IsEnabled = false;
                    AddressError.Text = Properties_Resources.InvalidURL;
                    return;
                }
            }
            FinishButton.IsEnabled = true;
            FinishButton.Focus();
            AddressError.Text = String.Empty;
        }

        private void ApplySetting()
        {
            ProxyNone.GroupName = ProxySystem.GroupName = ProxyCustom.GroupName = "proxy";
            ProxyNone.Content = Properties_Resources.NetworkProxySelectNone;
            ProxySystem.Content = Properties_Resources.NetworkProxySelectSystem;
            ProxyCustom.Content = Properties_Resources.NetworkProxySelectCustom;
            ProxyNone.Checked += delegate
            {
                SelectProxy();
            };
            ProxySystem.Checked += delegate
            {
                SelectProxy();
            };
            ProxyCustom.Checked += delegate
            {
                SelectProxy();
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

            AddressLabel.Text = Properties_Resources.NetworkProxyServer + ":";
            UserLabel.Text = Properties_Resources.User + ":";
            PasswordLabel.Text = Properties_Resources.Password + ":";

            AddressText.TextChanged += delegate(object sender, TextChangedEventArgs e)
            {
                CheckAddress();
            };

            FinishButton.Content = Properties_Resources.SaveChanges;
            CancelButton.Content = Properties_Resources.DiscardChanges;
        }
    }
}
