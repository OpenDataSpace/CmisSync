using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CmisSync.Lib.Events
{

    public class ActiveActivitiesManager
    {
        private object Lock = new object();
        private HashSet<FileTransmissionEvent> Transmissions = new HashSet<FileTransmissionEvent>();

        /// <summary>
        /// Add a new Transmission to the active transmission manager
        /// </summary>
        /// <param name="transmission"></param>
        public bool AddTransmission(FileTransmissionEvent transmission) {
            bool added = false;
            lock (Lock)
            {
                added = this.Transmissions.Add(transmission);
                transmission.TransmissionStatus += TransmissionFinished;
            }
            return added;
        }

        private void TransmissionFinished(object sender, TransmissionProgressEventArgs e)
        {
            if ((e.Aborted == true || e.Completed == true))
            {
                lock (Lock)
                {
                    this.Transmissions.Remove(sender as FileTransmissionEvent);
                }
            }
        }
    }
}
