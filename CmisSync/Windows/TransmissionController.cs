using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CmisSync.Lib.Events;


namespace CmisSync
{
    class TransmissionController
    {
        public event Action ShowWindowEvent = delegate { };
        public event Action HideWindowEvent = delegate { };

        public event Action<FileTransmissionEvent> UpdateTransmissionEvent = delegate { };

        public TransmissionController()
        {
            Program.Controller.ShowTransmissionWindowEvent += delegate
            {
                ShowWindowEvent();
            };
        }

        public void ShowWindow()
        {
            this.ShowWindowEvent();
        }

        public void HideWindow()
        {
            this.HideWindowEvent();
        }
    }
}
