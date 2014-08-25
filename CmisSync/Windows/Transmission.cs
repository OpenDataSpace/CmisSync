using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using System.Windows;


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
            Height = 480;
            Width = 640;
            Closing += Transmission_Closing;
            Controller.ShowWindowEvent += Controller_ShowWindowEvent;
            Controller.HideWindowEvent += Controller_HideWindowEvent;
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
    }
}
