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
    /// Interaction logic for SetupTutorialSecondWPF.xaml
    /// </summary>
    public partial class SetupTutorialSecondWPF : UserControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SetupTutorialSecondWPF()
        {
            InitializeComponent();
            ApplyTutorialSecond();
        }

        private void ApplyTutorialSecond()
        {
            slide_image.Source = UIHelpers.GetImageSource("tutorial-slide-2");
            continue_button.Content = Properties_Resources.Continue;
            continue_button.Focus();
        }

    }
}
