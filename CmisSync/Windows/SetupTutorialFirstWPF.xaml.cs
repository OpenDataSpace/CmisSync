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
    public partial class SetupTutorialFirstWPF : UserControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SetupTutorialFirstWPF()
        {
            InitializeComponent();
            ApplyTutorialFirst();
        }

        private void ApplyTutorialFirst()
        {
            slide_image.Source = UIHelpers.GetImageSource("tutorial-slide-1");
            continue_button.Content = Properties_Resources.Continue;
            cancel_button.Content = Properties_Resources.SkipTutorial;
            continue_button.Focus();
        }
    }
}
