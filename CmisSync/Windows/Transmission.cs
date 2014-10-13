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

        public class TransmissionData : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged = delegate { };

            public TransmissionData(TransmissionItem item)
            {
                Update(item);
            }

            public DateTime UpdateTime { get; private set; }
            public string FullPath { get; private set; }
            public string Repo { get; private set; }
            public string Path { get; private set; }
            public string Status { get; private set; }
            public string Progress { get; private set; }
            public bool Done { get; private set; }

            public void Update(TransmissionItem item)
            {
                UpdateTime = item.UpdateTime;
                FullPath = item.FullPath;
                Repo = item.Repo;
                Path = item.Path;
                Status = item.Status;
                Progress = item.Progress;
                Done = item.Done;
                var changeHandler = PropertyChanged;
                if (changeHandler != null) {
                    changeHandler(this, new PropertyChangedEventArgs("Repo"));
                    changeHandler(this, new PropertyChangedEventArgs("Path"));
                    changeHandler(this, new PropertyChangedEventArgs("Status"));
                    changeHandler(this, new PropertyChangedEventArgs("Progress"));
                }
            }
        }

        private ObservableCollection<TransmissionData> TransmissionList = new ObservableCollection<TransmissionData>();

        private void Controller_DeleteTransmissionEvent(TransmissionItem item)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                for (int i = TransmissionList.Count - 1; i >= 0; --i)
                {
                    if (TransmissionList[i].FullPath == item.FullPath)
                    {
                        TransmissionList.RemoveAt(i);
                        return;
                    }
                }
            });
        }

        private void Controller_InsertTransmissionEvent(TransmissionItem item)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                TransmissionList.Insert(0, new TransmissionData(item));
            });
        }

        private void Controller_UpdateTransmissionEvent(TransmissionItem item)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                for (int i = 0; i < TransmissionList.Count; ++i)
                {
                    if (TransmissionList[i].FullPath == item.FullPath)
                    {
                        TransmissionList[i].Update(item);
                        if (item.Done)
                        {
                            //  put finished TransmissionData to the tail
                            for (; i + 1 < TransmissionList.Count; ++i)
                            {
                                if (TransmissionList[i + 1].Done)
                                {
                                    break;
                                }
                                TransmissionData data = TransmissionList[i];
                                TransmissionList[i] = TransmissionList[i + 1];
                                TransmissionList[i + 1] = data;
                            }
                        }
                        ListView_SelectionChanged(this, null);
                        return;
                    }
                }
            });
        }

        private ListView ListView;
        private Button OkButton;

        private void LoadTransmission()
        {
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

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool open = false;
            foreach (object item in ListView.SelectedItems)
            {
                Transmission.TransmissionData data = item as Transmission.TransmissionData;
                if (data.Done)
                {
                    open = true;
                    break;
                }
            }
            MenuItem openMenu = ListView.FindResource("ListViewItemContextMenuOpen") as MenuItem;
            if (openMenu != null)
            {
                openMenu.IsEnabled = open;
            }
        }
    }
}
