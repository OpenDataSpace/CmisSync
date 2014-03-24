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
    /// Interaction logic for SetupTutorialThirdWPF.xaml
    /// </summary>
    public partial class SetupTutorialFourthWPF : UserControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SetupTutorialFourthWPF()
        {
            InitializeComponent();
            ApplyTutorialFourth();
        }

        private void ApplyTutorialFourth()
        {
            slide_image.Source = UIHelpers.GetImageSource("tutorial-slide-4");
            continue_button.Content = Properties_Resources.Finish;
            check_box.Content = String.Format(Properties_Resources.Startup, Properties_Resources.ApplicationName);
            continue_button.Focus();
        }

    }
}
