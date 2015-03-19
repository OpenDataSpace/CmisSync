
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
        /// Gets or sets the bits per second. Can be null if it is unknown.
        /// </summary>
        /// <value>
        /// The bits per second or null.
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