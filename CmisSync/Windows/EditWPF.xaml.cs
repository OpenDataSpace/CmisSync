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
    /// Interaction logic for EditWPF.xaml
    /// </summary>
    public partial class EditWPF : UserControl
    {
        public EditWPF()
        {
            InitializeComponent();
            ApplyEdit();
        }

        private void ApplyEdit()
        {
            addressLabel.Text = Properties_Resources.CmisWebAddress + ":";
            addressLabel.FontWeight = FontWeights.Bold;

            addressBox.IsEnabled = false;

            userLabel.Text = Properties_Resources.User + ":";
            userLabel.FontWeight = FontWeights.Bold;

            userBox.IsEnabled = false;

            passwordLabel.Text = Properties_Resources.Password + ":";
            passwordLabel.FontWeight = FontWeights.Bold;

            finishButton.Content = Properties_Resources.SaveChanges;
            cancelButton.Content = Properties_Resources.DiscardChanges;

            finishButton.Focus();
        }
    }
}
