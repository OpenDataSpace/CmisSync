
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