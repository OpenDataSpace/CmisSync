//-----------------------------------------------------------------------
// <copyright file="BandwidthNotifyingStream.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Streams {
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Timers;

    public class BandwidthNotifyingStream : StreamWrapper, INotifyPropertyChanged {
        /// <summary>
        /// The start time of the usage.
        /// </summary>
        private DateTime start = DateTime.Now;

        /// <summary>
        /// The bytes transmitted since last second.
        /// </summary>
        private long bytesTransmittedSinceLastSecond = 0;

        /// <summary>
        /// The blocking detection timer.
        /// </summary>
        private Timer blockingDetectionTimer;

        private long bitsPerSecond = 0;

        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public BandwidthNotifyingStream(Stream s) : base(s) {
            this.blockingDetectionTimer = new Timer(2000);
            this.blockingDetectionTimer.Elapsed += delegate(object sender, ElapsedEventArgs args) {
                this.BitsPerSecond = (long)((this.bytesTransmittedSinceLastSecond * 8) / this.blockingDetectionTimer.Interval);
                this.bytesTransmittedSinceLastSecond = 0;
            };

        }

        /// <summary>
        /// Gets or sets the bits per second.
        /// </summary>
        /// <value>
        /// The bits per second.
        /// </value>
        public long BitsPerSecond {
            get {
                return this.bitsPerSecond;
            }

            set {
                if (this.bitsPerSecond != value) {
                    this.bitsPerSecond = value;
                    this.NotifyPropertyChanged(Utils.NameOf(() => this.BitsPerSecond));
                }
            }
        }

        /// <summary>
        /// This method is called by the Set accessor of each property.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        private void NotifyPropertyChanged(string propertyName) {
            if (string.IsNullOrEmpty(propertyName)) {
                throw new ArgumentNullException("Given property name is null");
            }

            var handler = this.PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}