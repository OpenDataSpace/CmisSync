//-----------------------------------------------------------------------
// <copyright file="TransmissionWidget.xaml.cs" company="GRAU DATA AG">
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
﻿
namespace CmisSync {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
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

    using CmisSync.Lib.FileTransmission;

    /// <summary>
    /// Interaction logic for TransmissionWidget.xaml
    /// </summary>
    public partial class TransmissionWidget : UserControl {
        public TransmissionWidget() {
            InitializeComponent();
            this.Expander.Header = Properties_Resources.TransmissionExceptionDetails;
        }

        private void openButton_Click(object sender, RoutedEventArgs e) {
            var transmission = this.DataContext as Transmission;
            if (transmission != null) {
                if (transmission.Done) {
                    try {
                        if (System.IO.File.Exists(transmission.Path)) {
                            Process.Start(transmission.Path);
                        }
                    } catch (Exception) {
                    }
                }
            }
        }
    }
}