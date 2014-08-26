using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Collections.ObjectModel;

using CmisSync.Lib.Events;


namespace CmisSync
{
    /// <summary>
    /// Tranmission widget
    /// </summary>
    public class Transmission : Window
    {
        private TransmissionController Controller = new TransmissionController();

        /// <summary>
        /// Constructor
        /// </summary>
        public Transmission()
        {
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

            OkButton.Click += delegate
            {
                Controller_HideWindowEvent();
            };
        }

        private void Transmission_Closing(object sender, CancelEventArgs e)
        {
            Controller.HideWindow();
            e.Cancel = true;
        }

        private void Controller_ShowWindowEvent()
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                Show();
                Activate();
                BringIntoView();
                if (WindowState == System.Windows.WindowState.Minimized)
                {
                    WindowState = System.Windows.WindowState.Normal;
                }
            });
        }

        private void Controller_HideWindowEvent()
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                Hide();
            });
        }

        private ObservableCollection<TransmissionItem> TransmissionItems = new ObservableCollection<TransmissionItem>();

        private void Controller_DeleteTransmissionEvent(TransmissionItem item)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                bool removed = false;
                do
                {
                    removed = false;
                    foreach (TransmissionItem i in TransmissionItems)
                    {
                        if (i.Path == item.Path)
                        {
                            TransmissionItems.Remove(i);
                            removed = true;
                            break;
                        }
                    }
                } while (removed);
                //TransmissionItems.Remove(item);
            });
        }

        private void Controller_InsertTransmissionEvent(TransmissionItem item)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                TransmissionItems.Insert(0, item);
            });
        }

        private void Controller_UpdateTransmissionEvent(TransmissionItem item)
        {
            Controller_DeleteTransmissionEvent(item);
            Controller_InsertTransmissionEvent(item);
        }

        private ListView ListView;
        private Button OkButton;

        private void LoadTransmission()
        {
            System.Uri resourceLocater = new System.Uri("/DataSpaceSync;component/TransmissionWPF.xaml", System.UriKind.Relative);
            UserControl wpf = Application.LoadComponent(resourceLocater) as UserControl;

            ListView = wpf.FindName("ListView") as ListView;
            Binding binding = new Binding();
            binding.Source = TransmissionItems;
            ListView.SetBinding(ListView.ItemsSourceProperty, binding);
            OkButton = wpf.FindName("OkButton") as Button;
            OkButton.Content = Properties_Resources.Finish;

            Content = wpf;
        }
    }
}
