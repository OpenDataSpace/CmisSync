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
    /// Interaction logic for SetupAddCustomizeWPF.xaml
    /// </summary>
    public partial class SetupAddCustomizeWPF : UserControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SetupAddCustomizeWPF()
        {
            InitializeComponent();
            ApplyCustomize();
        }

        private void ApplyCustomize()
        {
            localfolder_label.Text = Properties_Resources.EnterLocalFolderName;
            localrepopath_label.Text = Properties_Resources.ChangeRepoPath;
            back_button.Content = Properties_Resources.Back;
            continue_button.Content = Properties_Resources.Add;
            cancel_button.Content = Properties_Resources.Cancel;
        }
    }
}
