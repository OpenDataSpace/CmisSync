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
    /// Interaction logic for AboutWPF.xaml
    /// </summary>
    public partial class AboutWPF : UserControl
    {
        public AboutWPF()
        {
            InitializeComponent();
            ApplyAbout();
        }

        private void ApplyAbout()
        {
            version.Foreground = new SolidColorBrush(Color.FromRgb(15, 133, 203));
            updates.Foreground = new SolidColorBrush(Color.FromRgb(15, 133, 203));
            credits.Foreground = new SolidColorBrush(Color.FromRgb(15, 133, 203));
            //Foreground = new SolidColorBrush(fontColor);
        }
    }
}
