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
            //ApplyAddLogin();
        }
        
        /*
        private void ApplyAddLogin()
        {
            address_label.Text = Properties_Resources.EnterWebAddress;
            //address_box.Text = (Controller.PreviousAddress != null) ? Controller.PreviousAddress.ToString() : String.Empty;
            address_help_label.Text = Properties_Resources.User + ":";

            user_label.Text = Properties_Resources.User + ":";
            user_box.Text = Properties_Resources.User + ":";
            user_help_label.Text = Properties_Resources.User + ":";

            password_label.Text = Properties_Resources.User + ":";
            //password_box.Password = Properties_Resources.User + ":";
            password_help_label.Text = Properties_Resources.User + ":";

            address_error_label.Text = Properties_Resources.User + ":";         

            continue_button.Content = Properties_Resources.SaveChanges;
            cancel_button.Content = Properties_Resources.DiscardChanges;
        }
        */
    }
}
