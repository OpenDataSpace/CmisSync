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

        private void SelectProxyCustom(bool select)
        {
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

        private void ApplySetting()
        {
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

            AddressLabel.Text = Properties_Resources.NetworkProxyServer + ":";
            UserLabel.Text = Properties_Resources.User + ":";
            PasswordLabel.Text = Properties_Resources.Password + ":";

            FinishButton.Content = Properties_Resources.SaveChanges;
            CancelButton.Content = Properties_Resources.DiscardChanges;
        }
    }
}
