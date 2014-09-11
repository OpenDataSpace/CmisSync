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
using System.Diagnostics;

namespace CmisSync
{
    /// <summary>
    /// Interaction logic for TransmissionWPF.xaml
    /// </summary>
    public partial class TransmissionWPF : UserControl
    {
        public TransmissionWPF()
        {
            InitializeComponent();
            ApplyTransmission();
        }

        private void ApplyTransmission()
        {
            ColumnRepo.Header = Properties_Resources.TransmissionTitleRepo;
            ColumnPath.Header = Properties_Resources.TransmissionTitlePath;
            ColumnStatus.Header = Properties_Resources.TransmissionTitleStatus;
            ColumnProgress.Header = Properties_Resources.TransmissionTitleProgress;
            OkButton.Content = Properties_Resources.Finish;
        }

        private void ListViewItem_Open(object sender, RoutedEventArgs e)
        {
            foreach (object item in ListView.SelectedItems)
            {
                Transmission.TransmissionData data = item as Transmission.TransmissionData;
                if (data.Done)
                {
                    Process.Start(data.FullPath);
                }
            }
        }

        private void ListViewItem_OpenFileLocation(object sender, RoutedEventArgs e)
        {
            foreach (object item in ListView.SelectedItems)
            {
                Transmission.TransmissionData data = item as Transmission.TransmissionData;
                Process.Start("explorer.exe", "/select,\"" + data.FullPath + "\"");
            }
        }
    }
}
