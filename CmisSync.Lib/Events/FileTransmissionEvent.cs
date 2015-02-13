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

namespace CmisSync.Lib.Events {
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// File transmission types.
    /// </summary>
    public enum FileTransmissionType {
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
    public class FileTransmissionEvent : ISyncEvent {
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
        public FileTransmissionEvent(FileTransmissionType type, string path, string cachePath = null) {
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
        public TransmissionProgressEventArgs Status {
            get { return this.status; }
            private set { this.status = value; }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.FileTransmissionEvent"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.FileTransmissionEvent"/>.
        /// </returns>
        public override string ToString() {
            return string.Format("FileTransmissionEvent with type \"{0}\" on path \"{1}\"", this.Type, this.Path);
        }

        /// <summary>
        /// Reports the progress. Every non null value will update the actual status.
        /// All other values will be taken from the last reported progress.
        /// </summary>
        /// <param name='status'>
        /// Status update.
        /// </param>
        public void ReportProgress(TransmissionProgressEventArgs status) {
            this.Status.Aborting = (status.Aborting != null) ? status.Aborting : this.Status.Aborting;
            this.Status.Aborted = (status.Aborted != null) ? status.Aborted : this.Status.Aborted;
            this.Status.ActualPosition = (status.ActualPosition != null) ? status.ActualPosition : this.Status.ActualPosition;
            this.Status.Length = (status.Length != null) ? status.Length : this.Status.Length;
            this.Status.Completed = (status.Completed != null) ? status.Completed : this.Status.Completed;
            this.Status.Started = (status.Started != null) ? status.Started : this.Status.Started;
            this.Status.BitsPerSecond = (status.BitsPerSecond != null) ? status.BitsPerSecond : this.Status.BitsPerSecond;
            this.Status.FailedException = (status.FailedException != null) ? status.FailedException : this.Status.FailedException;
            this.Status.Paused = (status.Paused != null) ? status.Paused : this.Status.Paused;
            if (this.TransmissionStatus != null) {
                this.TransmissionStatus(this, this.Status);
            }
        }
    }
 }