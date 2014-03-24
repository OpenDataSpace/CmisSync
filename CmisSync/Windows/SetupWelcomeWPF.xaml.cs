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
    /// Interaction logic for SetupWelcomeWPF.xaml
    /// </summary>
    public partial class SetupWelcomeWPF : UserControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SetupWelcomeWPF()
        {
            InitializeComponent();
            ApplySetupWelcome();
        }

        private void ApplySetupWelcome()
        {
            continue_button.Content = Properties_Resources.Continue;
            cancel_button.Content = Properties_Resources.Cancel;
            continue_button.Focus();
        }
    }
}
