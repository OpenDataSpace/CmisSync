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
    public partial class SetupTutorialThirdWPF : UserControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SetupTutorialThirdWPF()
        {
            InitializeComponent();
            ApplyTutorialThird();
        }

        private void ApplyTutorialThird()
        {
            slide_image.Source = UIHelpers.GetImageSource("tutorial-slide-3");
            continue_button.Content = Properties_Resources.Continue;
            continue_button.Focus();
        }
    }
}
