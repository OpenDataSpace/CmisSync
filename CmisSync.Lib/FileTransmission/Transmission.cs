//-----------------------------------------------------------------------
// <copyright file="Transmission.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.FileTransmission {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Text;

    using CmisSync.Lib;
    using CmisSync.Lib.Streams;

    /// <summary>
    /// File transmission types.
    /// </summary>
    public enum TransmissionType {
        /// <summary>
        /// A new file is uploaded
        /// </summary>
        UploadNewFile,

        /// <summary>
        /// A locally modified file is uploaded
        /// </summary>
        UploadModifiedFile,

        /// <summary>
        /// A new remote file is downloaded
        /// </summary>
        DownloadNewFile,

        /// <summary>
        /// A remotely modified file is downloaded
        /// </summary>
        DownloadModifiedFile
    }

    /// <summary>
    /// Transmission status.
    /// </summary>
    public enum TransmissionStatus {
        /// <summary>
        /// Transmission is going on.
        /// </summary>
        Transmitting,

        /// <summary>
        /// Transmission is requested to be aborted.
        /// </summary>
        Aborting,

        /// <summary>
        /// Transmission is aborted.
        /// </summary>
        Aborted,

        /// <summary>
        /// Transmission is paused.
        /// </summary>
        Paused,

        /// <summary>
        /// Transmission is finished successfully
        /// </summary>
        Finished
    }

    /// <summary>
    /// File transmission event.
    /// This event should be queued only once. The progress will not be reported on the queue.
    /// Interested entities should add themselfs as TransmissionEventHandler on the event TransmissionStatus to get informed about the progress.
    /// </summary>
    public class Transmission : INotifyPropertyChanged {
        private readonly TransmissionType type;
        private string relativePath = string.Empty;
        private string repo = string.Empty;
        private TransmissionStatus status;
        private long? length;
        private long? position;
        private long? bitsPerSecond;
        private Exception failedException;
        private DateTime lastModification = DateTime.Now;
        private long maxBandwidth;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.FileTransmission.Transmission"/> class.
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
        public Transmission(TransmissionType type, string path, string cachePath = null) {
            if (path == null) {
                throw new ArgumentNullException("path");
            }

            this.type = type;
            this.Path = path;
            this.CachePath = cachePath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.FileTransmission.Transmission"/> class, but should only be used by UI templates in design phase.
        /// </summary>
        [Obsolete("Should only be used by UI templates", true)]
        public Transmission() {
            this.Path = "Not Set";
            this.CachePath = null;
        }

        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the type of the transmission.
        /// </summary>
        /// <value>
        /// The type of the transmission.
        /// </value>
        public TransmissionType Type {
            get {
                return this.type;
            }
        }

        /// <summary>
        /// Gets or sets the repository.
        /// </summary>
        /// <value>The repository.</value>
        public string Repository {
            get {
                return this.repo;
            }

            set {
                if (this.repo != value) {
                    this.repo = value;
                    this.NotifyPropertyChanged(Utils.NameOf(() => this.Repository));
                }
            }
        }

        /// <summary>
        /// Gets or sets the relative path.
        /// </summary>
        /// <value>The relative path.</value>
        public string RelativePath {
            get {
                return this.relativePath;
            }

            set {
                if (this.relativePath != value) {
                    this.relativePath = value;
                    this.NotifyPropertyChanged(Utils.NameOf(() => this.RelativePath));
                }
            }
        }

        /// <summary>
        /// Gets the path to the file, which is transmitted.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; private set; }

        /// <summary>
        /// Gets the file name of the target file
        /// </summary>
        public string FileName {
            get {
                return new System.IO.FileInfo(this.Path).Name;
            }
        }

        /// <summary>
        /// Gets download cache file. If a download happens, a cache file could be used. If the cache is used, this should be the path.
        /// </summary>
        /// <value>
        /// The cache path.
        /// </value>
        public string CachePath { get; private set; }

        /// <summary>
        /// Gets or sets the length of the file transmission in bytes.
        /// </summary>
        /// <value>
        /// The transmission length.
        /// </value>
        public long? Length {
            get {
                return this.length;
            }

            set {
                if (this.length != value) {
                    this.length = value;
                    this.LastModification = DateTime.Now;
                    this.NotifyPropertyChanged(Utils.NameOf(() => this.Length));
                    this.NotifyPropertyChanged(Utils.NameOf(() => this.Percent));
                }
            }
        }

        /// <summary>
        /// Gets or sets the actual position of the transmission progress.
        /// </summary>
        /// <value>
        /// The actual transmission position.
        /// </value>
        public long? Position {
            get {
                return this.position;
            }

            set {
                if (this.position != value) {
                    var percent = this.Percent;
                    this.position = value;
                    this.LastModification = DateTime.Now;
                    this.NotifyPropertyChanged(Utils.NameOf(() => this.Position));
                    if (percent != this.Percent) {
                        this.NotifyPropertyChanged(Utils.NameOf(() => this.Percent));
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the status of the transmission.
        /// </summary>
        /// <value>The status.</value>
        public TransmissionStatus Status {
            get {
                return this.status;
            }

            set {
                if (this.status != value) {
                    this.status = value;
                    this.LastModification = DateTime.Now;
                    this.NotifyPropertyChanged(Utils.NameOf(() => this.Status));
                    if (this.Done) {
                        this.BitsPerSecond = null;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the failed exception of the transmission, if any exception occures.
        /// </summary>
        /// <value>
        /// Transmission failed exception.
        /// </value>
        public Exception FailedException {
            get {
                return this.failedException;
            }

            set {
                if (this.failedException != value) {
                    this.failedException = value;
                    this.NotifyPropertyChanged(Utils.NameOf(() => this.FailedException));
                    this.Status = TransmissionStatus.Aborted;
                }
            }
        }

        /// <summary>
        /// Gets or sets the bits per second. Can be null if it is unknown.
        /// </summary>
        /// <value>
        /// The bits per second or null.
        /// </value>
        public long? BitsPerSecond {
            get {
                return this.bitsPerSecond;
            }

            set {
                if (this.bitsPerSecond != value && !this.Done) {
                    this.bitsPerSecond = value;
                    this.NotifyPropertyChanged(Utils.NameOf(() => this.BitsPerSecond));
                }
            }
        }

        /// <summary>
        /// Gets the percentage of the transmission progress if known. Otherwise null.
        /// </summary>
        /// <value>
        /// The percentage of the transmission progress.
        /// </value>
        public double? Percent {
            get {
                if (this.Length == null || this.Position == null || this.Position < 0 || this.Length < 0) {
                    return null;
                }

                if (this.Length == 0) {
                    return 100d;
                }

                return Math.Round(((double)this.Position * 100d) / (double)this.Length, 0);
            }
        }

        /// <summary>
        /// Gets or sets the last modification of this transmission.
        /// </summary>
        /// <value>The last modification.</value>
        public DateTime LastModification {
            get {
                return this.lastModification;
            }

            set {
                if (Math.Abs((value - this.lastModification).TotalSeconds) > 1) {
                    this.lastModification = value;
                    this.NotifyPropertyChanged(Utils.NameOf(() => this.LastModification));
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum bandwidth. Zero or negative value will disable the limitations.
        /// </summary>
        /// <value>The max bandwidth.</value>
        public long MaxBandwidth {
            get {
                return this.maxBandwidth;
            }

            set {
                if (this.maxBandwidth != value) {
                    this.maxBandwidth = value;
                    this.NotifyPropertyChanged(Utils.NameOf(() => this.MaxBandwidth));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="CmisSync.Lib.FileTransmission.Transmission"/> is done.
        /// It is true if the transmission is aborted or finished, otherwise false.
        /// </summary>
        /// <value><c>true</c> if done; otherwise, <c>false</c>.</value>
        public bool Done {
            get {
                return this.Status == TransmissionStatus.Aborted || this.Status == TransmissionStatus.Finished;
            }
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="CmisSync.Lib.FileTransmission.TransmissionController"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode() {
            return base.GetHashCode();
        }

        /// <summary>
        /// Pause the transmission async.
        /// </summary>
        public void Pause() {
            if (this.Status == TransmissionStatus.Transmitting) {
                this.Status = TransmissionStatus.Paused;
            }
        }

        /// <summary>
        /// Resume the transmission async.
        /// </summary>
        public void Resume() {
            if (this.Status == TransmissionStatus.Paused) {
                this.Status = TransmissionStatus.Transmitting;
            }
        }

        /// <summary>
        /// Abort the transmission async.
        /// </summary>
        public void Abort() {
            if (this.Status == TransmissionStatus.Paused || this.Status == TransmissionStatus.Transmitting) {
                this.Status = TransmissionStatus.Aborting;
            }
        }

        /// <summary>
        /// Creates the stream.
        /// </summary>
        /// <returns>The stream.</returns>
        /// <param name="wrappedStream">Wrapped stream.</param>
        public Stream CreateStream(Stream wrappedStream) {
            return new TransmissionStream(wrappedStream, this);
        }

        /// <summary>
        /// This method is called by the Set accessor of each property.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        private void NotifyPropertyChanged(string propertyName) {
            if (string.IsNullOrEmpty(propertyName)) {
                throw new ArgumentNullException("propertyName");
            }

            var handler = this.PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
 }