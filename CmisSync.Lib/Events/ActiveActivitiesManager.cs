using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using log4net;

namespace CmisSync.Lib.Events
{

    public class ActiveActivitiesManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ActiveActivitiesManager));

        private object Lock = new object();
        private HashSet<FileTransmissionEvent> Transmissions = new HashSet<FileTransmissionEvent>();
        private ObservableCollection<FileTransmissionEvent> activeTransmissions = new ObservableCollection<FileTransmissionEvent>();
        public ObservableCollection<FileTransmissionEvent> ActiveTransmissions { get { return activeTransmissions; } }

        /// <summary>
        /// Add a new Transmission to the active transmission manager
        /// </summary>
        /// <param name="transmission"></param>
        public bool AddTransmission(FileTransmissionEvent transmission) {
            bool added = false;
            lock (Lock)
            {
                added = this.Transmissions.Add(transmission);
                if(added) {
                    transmission.TransmissionStatus += TransmissionFinished;
                    activeTransmissions.Add(transmission);
                }
            }
            return added;
        }

        private void TransmissionFinished(object sender, TransmissionProgressEventArgs e)
        {
            if ((e.Aborted == true || e.Completed == true || e.FailedException != null))
            {
                lock (Lock)
                {
                    FileTransmissionEvent transmission = sender as FileTransmissionEvent;
                    if(transmission!=null && this.Transmissions.Remove(transmission)) {
                        activeTransmissions.Remove(transmission);
                        transmission.TransmissionStatus-=TransmissionFinished;
                        Logger.Debug("Transmission removed");
                    }
                }
            }
        }
    }
}
