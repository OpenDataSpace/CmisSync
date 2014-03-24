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
    /// Interaction logic for SetupAddLoginWPF.xaml
    /// </summary>
    public partial class SetupAddLoginWPF : UserControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SetupAddLoginWPF()
        {
            InitializeComponent();
            ApplyAddLogin();
        }
        
        private void ApplyAddLogin()
        {
            address_label.Text = Properties_Resources.EnterWebAddress;
            address_help_label.Text = Properties_Resources.Help + ": ";
            Run run = new Run(Properties_Resources.WhereToFind);
            Hyperlink link = new Hyperlink(run);
            link.NavigateUri = new Uri("https://github.com/nicolas-raoul/CmisSync/wiki/What-address");
            address_help_label.Inlines.Add(link);
            link.RequestNavigate += (sender, e) =>
            {
                System.Diagnostics.Process.Start(e.Uri.ToString());
            };

            user_label.Text = Properties_Resources.User + ":";
            password_label.Text = Properties_Resources.Password + ":";

            continue_button.Content = Properties_Resources.Continue;
            cancel_button.Content = Properties_Resources.Cancel;
        }
    }
}
