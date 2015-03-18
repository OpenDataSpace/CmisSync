//-----------------------------------------------------------------------
// <copyright file="ActiveActivitiesManager.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Queueing {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.FileTransmission;

    using log4net;

    /// <summary>
    /// Transmission manager.
    /// </summary>
    public class TransmissionManager : ITransmissionManager {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TransmissionManager));

        private object collectionLock = new object();
        private ObservableCollection<Transmission> activeTransmissions = new ObservableCollection<Transmission>();

        /// <summary>
        /// Gets the active transmissions. This Collection can be obsered for changes.
        /// </summary>
        /// <value>
        /// The active transmissions.
        /// </value>
        public ObservableCollection<Transmission> ActiveTransmissions {
            get {
                return this.activeTransmissions;
            }
        }

        /// <summary>
        /// Active the transmissions as list.
        /// </summary>
        /// <returns>
        /// The transmissions as list.
        /// </returns>
        public List<Transmission> ActiveTransmissionsAsList() {
            lock (this.collectionLock) {
                return this.activeTransmissions.ToList<Transmission>();
            }
        }

        /// <summary>
        /// Add a new Transmission to the active transmission manager
        /// </summary>
        /// <param name="transmission">transmission which should be added</param>
        /// <returns>true if added</returns>
        public virtual bool AddTransmission(Transmission transmission) {
            if (transmission == null) {
                throw new ArgumentNullException();
            }

            lock (this.collectionLock) {
                if (this.activeTransmissions.Contains(transmission)) {
                    return false;
                }

                transmission.PropertyChanged += this.TransmissionFinished;
                this.activeTransmissions.Add(transmission);
            }

            return true;
        }

        public Transmission CreateTransmission(TransmissionType type, string path, string cachePath = null) {
            var transmission = new Transmission(type, path, cachePath);
            transmission.PropertyChanged += this.TransmissionFinished;
            this.activeTransmissions.Add(transmission);
            return transmission;
        }


        /// <summary>
        /// Aborts all open HTTP requests.
        /// </summary>
        public void AbortAllRequests() {
            DotCMIS.Binding.HttpWebRequestResource.AbortAll();
        }

        /// <summary>
        /// If a transmission is reported as finished/aborted/failed, the transmission is removed from the collection
        /// </summary>
        /// <param name='sender'>
        /// The transmission event.
        /// </param>
        /// <param name='e'>
        /// The progress parameters of the transmission.
        /// </param>
        private void TransmissionFinished(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName != Utils.NameOf((Transmission t) => t.Status)) {
                return;
            }

            var transmission = (sender as Transmission);
            if (transmission != null &&
                (transmission.Status == TransmissionStatus.ABORTED || transmission.Status == TransmissionStatus.FINISHED)) {
                lock (this.collectionLock) {
                    this.activeTransmissions.Remove(transmission);
                    transmission.PropertyChanged -= TransmissionFinished;
                    Logger.Debug("Transmission removed");
                }
            }
        }
    }
}