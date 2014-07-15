//-----------------------------------------------------------------------
// <copyright file="FileTransmissionEvent.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Events
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// File transmission types.
    /// </summary>
    public enum FileTransmissionType
    {
        /// <summary>
        /// A new file is uploaded
        /// </summary>
        UPLOAD_NEW_FILE,

        /// <summary>
        /// A locally modified file is uploaded
        /// </summary>
        UPLOAD_MODIFIED_FILE,

        /// <summary>
        /// A new remote file is downloaded
        /// </summary>
        DOWNLOAD_NEW_FILE,

        /// <summary>
        /// A remotely modified file is downloaded
        /// </summary>
        DOWNLOAD_MODIFIED_FILE
    }

    /// <summary>
    /// File transmission event.
    /// This event should be queued only once. The progress will not be reported on the queue.
    /// Interested entities should add themselfs as TransmissionEventHandler on the event TransmissionStatus to get informed about the progress.
    /// </summary>
    public class FileTransmissionEvent : ISyncEvent
    {
        private TransmissionProgressEventArgs status;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.FileTransmissionEvent"/> class.
        /// </summary>
        /// <param name='type'>
        /// Type of the transmission.
        /// </param>
        /// <param name='path'>
        /// Path to the file of the transmission.
        /// </param>
        /// <param name='cachePath'>
        /// If a download runs and a cache file is used, this should be the path to the cache file
        /// </param>
        public FileTransmissionEvent(FileTransmissionType type, string path, string cachePath = null)
        {
            if (path == null) {
                throw new ArgumentNullException("Argument null in FSEvent Constructor", "path");
            }

            this.Type = type;
            this.Path = path;
            this.status = new TransmissionProgressEventArgs();
            this.CachePath = cachePath;
        }

        public delegate void TransmissionEventHandler(object sender, TransmissionProgressEventArgs e);

        /// <summary>
        /// Occurs when transmission status changes.
        /// </summary>
        public event TransmissionEventHandler TransmissionStatus = delegate { };

        /// <summary>
        /// Gets the type of the transmission.
        /// </summary>
        /// <value>
        /// The type of the transmission.
        /// </value>
        public FileTransmissionType Type { get; private set; }

        /// <summary>
        /// Gets the path to the file, which is transmitted.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; private set; }

        /// <summary>
        /// Gets download cache file. If a download happens, a cache file could be used. If the cache is used, this should be the path.
        /// </summary>
        /// <value>
        /// The cache path.
        /// </value>
        public string CachePath { get; private set; }

        /// <summary>
        /// Gets the actual status of the transmission.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public TransmissionProgressEventArgs Status
        {
            get { return this.status; }
            private set { this.status = value; }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.FileTransmissionEvent"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.FileTransmissionEvent"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("FileTransmissionEvent with type \"{0}\" on path \"{1}\"", this.Type, this.Path);
        }

        /// <summary>
        /// Reports the progress. Every non null value will update the actual status.
        /// All other values will be taken from the last reported progress.
        /// </summary>
        /// <param name='status'>
        /// Status update.
        /// </param>
        public void ReportProgress(TransmissionProgressEventArgs status)
        {
            this.Status.Aborting = (status.Aborting != null) ? status.Aborting : this.Status.Aborting;
            this.Status.Aborted = (status.Aborted != null) ? status.Aborted : this.Status.Aborted;
            this.Status.ActualPosition = (status.ActualPosition != null) ? status.ActualPosition : this.Status.ActualPosition;
            this.Status.Length = (status.Length != null) ? status.Length : this.Status.Length;
            this.Status.Completed = (status.Completed != null) ? status.Completed : this.Status.Completed;
            this.Status.Started = (status.Started != null) ? status.Started : this.Status.Started;
            this.Status.BitsPerSecond = (status.BitsPerSecond != null) ? status.BitsPerSecond : this.Status.BitsPerSecond;
            this.Status.FailedException = (status.FailedException != null) ? status.FailedException : this.Status.FailedException;
            if (this.TransmissionStatus != null) {
                this.TransmissionStatus(this, this.Status);
            }
        }
    }

    /// <summary>
    /// Transmission progress event arguments.
    /// </summary>
    public class TransmissionProgressEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.TransmissionProgressEventArgs"/> class.
        /// </summary>
        public TransmissionProgressEventArgs()
        {
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

                if (this.Length == 0)
                {
                    return 100d;
                }

                return ((double)this.ActualPosition * 100d) / (double)this.Length;
            } }

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
            if (obj == null)
            {
                return false;
            }

            TransmissionProgressEventArgs e = obj as TransmissionProgressEventArgs;
            if ((object)e == null)
            {
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
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.TransmissionProgressEventArgs"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.TransmissionProgressEventArgs"/>.
        /// </returns>
        public override string ToString()
        {
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
