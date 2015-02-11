//-----------------------------------------------------------------------
// <copyright file="SetupAddLoginWPF.xaml.cs" company="GRAU DATA AG">
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
