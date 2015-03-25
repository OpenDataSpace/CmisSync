//-----------------------------------------------------------------------
// <copyright file="ProgressStream.cs" company="GRAU DATA AG">
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
    using System.IO;
    using System.Timers;

    using CmisSync.Lib.FileTransmission;

    /// <summary>
    /// Progress reporting stream.
    /// </summary>
    public class ProgressStream : NotifyPropertyChangedStream {
        /// <summary>
        /// The transmission controller which is used to report the status.
        /// </summary>
        private Transmission transmission;

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

        /// <summary>
        /// The length of the underlaying stream.
        /// </summary>
        private long length;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Streams.ProgressStream"/> class.
        /// The given transmission event will be used to report the progress
        /// </summary>
        /// <param name='stream'>
        /// Stream which progress should be monitored.
        /// </param>
        /// <param name='e'>
        /// Transmission event where the progress should be reported to.
        /// </param>
        public ProgressStream(Stream stream, Transmission transmission) : base(stream) {
            if (transmission == null) {
                throw new ArgumentNullException("The event, where to publish the prgress cannot be null");
            }

            try {
                transmission.Length = stream.Length;
            } catch (NotSupportedException) {
                transmission.Length = null;
            }

            this.transmission = transmission;
            this.blockingDetectionTimer = new Timer(2000);
            this.blockingDetectionTimer.Elapsed += delegate(object sender, ElapsedEventArgs args) {
                this.transmission.BitsPerSecond = (long)((this.bytesTransmittedSinceLastSecond * 8) / this.blockingDetectionTimer.Interval);
                this.bytesTransmittedSinceLastSecond = 0;
            };
        }

        #region overrideCode
        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public override long Length {
            get {
                var newLength = this.Stream.Length;
                if (this.length != newLength) {
                    this.length = newLength;
                }

                return this.length;
            }
        }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public override long Position {
            get {
                long pos = this.Stream.Position;
                if (pos != this.transmission.Position) {
                    this.transmission.Position = pos;
                }

                return pos;
            }

            set {
                this.Stream.Position = value;
                if (value != this.transmission.Position) {
                    this.transmission.Position = value;
                }
            }
        }

        /// <summary>
        /// Seek the specified offset and origin.
        /// </summary>
        /// <param name='offset'>
        /// Offset.
        /// </param>
        /// <param name='origin'>
        /// Origin.
        /// </param>
        public override long Seek(long offset, SeekOrigin origin) {
            long result = this.Stream.Seek(offset, origin);
            this.transmission.Position = this.Stream.Position;
            return result;
        }

        /// <summary>
        /// Read the specified buffer, offset and count.
        /// </summary>
        /// <param name='buffer'>
        /// Buffer.
        /// </param>
        /// <param name='offset'>
        /// Offset.
        /// </param>
        /// <param name='count'>
        /// Count.
        /// </param>
        public override int Read(byte[] buffer, int offset, int count) {
            if (this.transmission.Status == TransmissionStatus.ABORTING) {
                this.transmission.Status = TransmissionStatus.ABORTED;
                throw new FileTransmission.AbortException(this.transmission.Path);
            }

            this.PauseIfRequested();

            int result = this.Stream.Read(buffer, offset, count);

            this.CalculateBandwidth(result);
            return result;
        }

        /// <summary>
        /// Sets the length.
        /// </summary>
        /// <param name='value'>
        /// Value.
        /// </param>
        public override void SetLength(long value) {
            this.Stream.SetLength(value);
            if (this.transmission.Length == null || value > (long)this.transmission.Length) {
                this.transmission.Length = value;
            }
        }

        /// <summary>
        /// Write the specified buffer, offset and count.
        /// </summary>
        /// <param name='buffer'>
        /// Buffer.
        /// </param>
        /// <param name='offset'>
        /// Offset.
        /// </param>
        /// <param name='count'>
        /// Count.
        /// </param>
        public override void Write(byte[] buffer, int offset, int count) {
            // for it may be chained before CryptoStream, we should write the content for CryptoStream has calculated the hash of the content
            this.Stream.Write(buffer, offset, count);
            this.CalculateBandwidth(count);

            if (this.transmission.Status == TransmissionStatus.ABORTING) {
                this.transmission.Status = TransmissionStatus.ABORTED;
                throw new FileTransmission.AbortException(this.transmission.Path);
            }

            this.PauseIfRequested();
        }
#endregion

        /// <summary>
        /// Close this instance and calculates the bandwidth of the last second.
        /// </summary>
        public override void Close() {
            long? result = Transmission.CalcBitsPerSecond(this.start, DateTime.Now.AddMilliseconds(1), this.bytesTransmittedSinceLastSecond);
            this.transmission.BitsPerSecond = result;
            this.blockingDetectionTimer.Stop();
            this.transmission.BitsPerSecond = null;
            base.Close();
        }

        /// <summary>
        /// Calculates the bandwidth.
        /// </summary>
        /// <param name='transmittedBytes'>
        /// Transmitted bytes.
        /// </param>
        private void CalculateBandwidth(int transmittedBytes) {
            this.bytesTransmittedSinceLastSecond += transmittedBytes;
            TimeSpan diff = DateTime.Now - this.start;
            long? pos;
            long? length = null;
            try {
                pos = Stream.Position;
                if (pos > this.transmission.Length) {
                    length = this.Stream.Length;
                }
            } catch (NotSupportedException) {
                pos = null;
            }

            this.transmission.Position = pos;
            this.transmission.Length = length;
            if (diff.Seconds >= 1) {
                long? result = Transmission.CalcBitsPerSecond(this.start, DateTime.Now, this.bytesTransmittedSinceLastSecond);
                this.transmission.BitsPerSecond = result;
                this.bytesTransmittedSinceLastSecond = 0;
                this.start = this.start + diff;
                this.blockingDetectionTimer.Stop();
                this.blockingDetectionTimer.Start();
            }
        }

        private void PauseIfRequested() {
            while (this.transmission.Status == TransmissionStatus.PAUSED) {
                System.Threading.Thread.Sleep(250);
            }
        }
    }
}