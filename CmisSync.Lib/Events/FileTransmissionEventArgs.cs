
namespace CmisSync.Lib.Events {
    using System;
    /// <summary>
    /// Transmission progress event arguments.
    /// </summary>
    public class TransmissionProgressEventArgs {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.TransmissionProgressEventArgs"/> class.
        /// </summary>
        public TransmissionProgressEventArgs() {
            this.BitsPerSecond = null;
            this.Length = null;
            this.ActualPosition = null;
            this.Paused = null;
            this.Resumed = null;
            this.Aborting = null;
            this.Aborted = null;
            this.FailedException = null;
        }

        /// <summary>
        /// Gets or sets the bits per second. Can be null if its unknown.
        /// </summary>
        /// <value>
        /// The bits per second or null.
        /// </value>
        public long? BitsPerSecond { get; set; }

        /// <summary>
        /// Gets the percentage of the transmission progress if known. Otherwise null.
        /// </summary>
        /// <value>
        /// The percentage of the transmission progress.
        /// </value>
        public double? Percent { get {
                if (this.Length == null || this.ActualPosition == null || this.ActualPosition < 0 || this.Length < 0) {
                    return null;
                }

                if (this.Length == 0) {
                    return 100d;
                }

                return ((double)this.ActualPosition * 100d) / (double)this.Length;
            }
        }

        /// <summary>
        /// Gets or sets the length of the file transmission in bytes.
        /// </summary>
        /// <value>
        /// The transmission length.
        /// </value>
        public long? Length { get; set; }

        /// <summary>
        /// Gets or sets the actual position of the transmission progress.
        /// </summary>
        /// <value>
        /// The actual transmission position.
        /// </value>
        public long? ActualPosition { get; set; }

        /// <summary>
        /// Gets or sets if the transmission is paused.
        /// </summary>
        /// <value>
        /// Transmission paused.
        /// </value>
        public bool? Paused { get; set; }

        /// <summary>
        /// Gets or sets if the transmission is resumed.
        /// </summary>
        /// <value>
        /// Transmission resumed.
        /// </value>
        public bool? Resumed { get; set; }

        /// <summary>
        /// Gets or sets if the transmission is aborting.
        /// </summary>
        /// <value>
        /// Transmission aborted.
        /// </value>
        public bool? Aborting { get; set; }

        /// <summary>
        /// Gets or sets if the transmission is aborted.
        /// </summary>
        /// <value>
        /// Transmission aborted.
        /// </value>
        public bool? Aborted { get; set; }

        /// <summary>
        /// Gets or sets if the transmission is started.
        /// </summary>
        /// <value>
        /// Transmission started.
        /// </value>
        public bool? Started { get; set; }

        /// <summary>
        /// Gets or sets if the transmission is completed.
        /// </summary>
        /// <value>
        /// Transmission completed.
        /// </value>
        public bool? Completed { get; set; }

        /// <summary>
        /// Gets or sets the failed exception of the transmission, if any exception occures.
        /// </summary>
        /// <value>
        /// Transmission failed exception.
        /// </value>
        public Exception FailedException { get; set; }

        /// <summary>
        /// Calculates the bits per second.
        /// </summary>
        /// <returns>
        /// The bits per second.
        /// </returns>
        /// <param name='start'>
        /// Start time for calculation.
        /// </param>
        /// <param name='end'>
        /// End time for calculation.
        /// </param>
        /// <param name='bytes'>
        /// Bytes in period between start end end.
        /// </param>
        public static long? CalcBitsPerSecond(DateTime start, DateTime end, long bytes) {
            if (end < start) {
                throw new ArgumentException("The end of a transmission must be higher than the start");
            }

            if (start == end) {
                return null;
            }

            TimeSpan difference = end - start;
            double seconds = difference.TotalMilliseconds / 1000d;
            double dbytes = bytes;
            return (long)((dbytes * 8) / seconds);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="CmisSync.Lib.Events.TransmissionProgressEventArgs"/>.
        /// </summary>
        /// <param name='obj'>
        /// The <see cref="System.Object"/> to compare with the current <see cref="CmisSync.Lib.Events.TransmissionProgressEventArgs"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="CmisSync.Lib.Events.TransmissionProgressEventArgs"/>; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) {
            // If parameter is null return false.
            if (obj == null) {
                return false;
            }

            TransmissionProgressEventArgs e = obj as TransmissionProgressEventArgs;
            if ((object)e == null) {
                return false;
            }

            // Return true if the fields match:
            return (this.Length == e.Length) &&
                (this.BitsPerSecond == e.BitsPerSecond || this.BitsPerSecond == null || e.BitsPerSecond == null) &&
                    (this.ActualPosition == e.ActualPosition) &&
                    (this.Paused == e.Paused) &&
                    (this.Resumed == e.Resumed) &&
                    (this.Aborting == e.Aborting) &&
                    (this.Aborted == e.Aborted) &&
                    (this.FailedException == e.FailedException);
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="CmisSync.Lib.Events.TransmissionProgressEventArgs"/> object.
        /// </summary>
        /// <returns>
        /// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.
        /// </returns>
        public override int GetHashCode() {
            return base.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.TransmissionProgressEventArgs"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.TransmissionProgressEventArgs"/>.
        /// </returns>
        public override string ToString() {
            string status = this.Paused == true ? "Paused" : string.Empty;
            status += this.Resumed == true ? "Resumed" : string.Empty;
            status += this.Aborting == true ? "Aborting" : string.Empty;
            status += this.Aborted == true ? "Aborted" : string.Empty;
            status += this.Completed == true ? "Completed" : string.Empty;
            return string.Format(
                "[TransmissionProgressEventArgs: [Length: {0}] [ActualPosition: {1}] [Percent: {2}] [Status: {3}]] [Exception: {4}]",
                this.Length,
                this.ActualPosition,
                this.Percent,
                status,
                this.FailedException.ToString());
        }
    }
}