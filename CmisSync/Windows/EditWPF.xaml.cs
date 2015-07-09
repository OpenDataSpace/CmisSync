//-----------------------------------------------------------------------
// <copyright file="EditWPF.xaml.cs" company="GRAU DATA AG">
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

namespace CmisSync {
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

    /// <summary>
    /// Interaction logic for EditWPF.xaml
    /// </summary>
    public partial class EditWPF : UserControl {
        public EditWPF() {
            InitializeComponent();
            ApplyEdit();
        }

        private void ApplyEdit() {
            this.addressLabel.Text = Properties_Resources.CmisWebAddress + ":";
            this.addressLabel.FontWeight = FontWeights.Bold;

            this.addressBox.IsEnabled = false;

            this.userLabel.Text = Properties_Resources.User + ":";
            this.userLabel.FontWeight = FontWeights.Bold;

            this.userBox.IsEnabled = false;

            this.passwordLabel.Text = Properties_Resources.Password + ":";
            this.passwordLabel.FontWeight = FontWeights.Bold;

            this.finishButton.Content = Properties_Resources.SaveChanges;
            this.cancelButton.Content = Properties_Resources.DiscardChanges;
            this.downloadLimitBox.Header = Properties_Resources.DownloadLimit;
            this.uploadLimitBox.Header = Properties_Resources.UploadLimit;
            this.tabItemBandwidth.Header = Properties_Resources.Bandwidth;

            this.finishButton.Focus();
        }
    }
}