//-----------------------------------------------------------------------
// <copyright file="AboutWPF.xaml.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------
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
