//-----------------------------------------------------------------------
// <copyright file="Transmission.cs" company="GRAU DATA AG">
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

namespace CmisSync {
    ﻿using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;

    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Controls;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.FileTransmission;

    /// <summary>
    /// Tranmission widget
    /// </summary>
    public class TransmissionWindow : Window {
        private TransmissionController Controller = new TransmissionController();

        /// <summary>
        /// Constructor
        /// </summary>
        public TransmissionWindow() {
            Title = Properties_Resources.Transmission;
            Height = 480;
            Width = 640;
            Icon = UIHelpers.GetImageSource("app", "ico");

            Closing += Transmission_Closing;
            Controller.ShowWindowEvent += Controller_ShowWindowEvent;
            Controller.HideWindowEvent += Controller_HideWindowEvent;
            Controller.InsertTransmissionEvent += Controller_InsertTransmissionEvent;
            Controller.UpdateTransmissionEvent += Controller_UpdateTransmissionEvent;
            Controller.DeleteTransmissionEvent += Controller_DeleteTransmissionEvent;

            LoadTransmission();

            OkButton.Click += delegate {
                Controller_HideWindowEvent();
            };
        }

        private void Transmission_Closing(object sender, CancelEventArgs e) {
            Controller.HideWindow();
            e.Cancel = true;
        }

        private void Controller_ShowWindowEvent() {
            Dispatcher.BeginInvoke((Action)delegate {
                Show();
                Activate();
                BringIntoView();
                if (WindowState == System.Windows.WindowState.Minimized) {
                    WindowState = System.Windows.WindowState.Normal;
                }
            });
        }

        private void Controller_HideWindowEvent() {
            Dispatcher.BeginInvoke((Action)delegate {
                Hide();
            });
        }

        private ObservableCollection<Transmission> TransmissionList = new ObservableCollection<Transmission>();

        private void Controller_DeleteTransmissionEvent(Transmission item) {
            Dispatcher.BeginInvoke((Action)delegate {
                this.TransmissionList.Remove(item);
            });
        }

        private void Controller_InsertTransmissionEvent(Transmission item) {
            Dispatcher.BeginInvoke((Action)delegate {
                TransmissionList.Insert(0, item);
            });
        }

        private void Controller_UpdateTransmissionEvent(Transmission item) {
            Dispatcher.BeginInvoke((Action)delegate {
                if (TransmissionList.Contains(item)) {
                    ListView_SelectionChanged(this, null);
                }
            });
        }

        private ListView ListView;
        private Button OkButton;

        private void LoadTransmission() {
            System.Uri resourceLocater = new System.Uri("/DataSpaceSync;component/TransmissionWPF.xaml", System.UriKind.Relative);
            UserControl wpf = Application.LoadComponent(resourceLocater) as UserControl;

            ListView = wpf.FindName("ListView") as ListView;
            Binding binding = new Binding();
            binding.Source = TransmissionList;
            ListView.SetBinding(ListView.ItemsSourceProperty, binding);
            ListView.SelectionChanged += ListView_SelectionChanged;

            OkButton = wpf.FindName("OkButton") as Button;

            Content = wpf;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            bool open = false;
            foreach (object item in ListView.SelectedItems) {
                Transmission data = item as Transmission;
                if (data.Done) {
                    open = true;
                    break;
                }
            }

            MenuItem openMenu = ListView.FindResource("ListViewItemContextMenuOpen") as MenuItem;
            if (openMenu != null) {
                openMenu.IsEnabled = open;
            }
        }
    }
}