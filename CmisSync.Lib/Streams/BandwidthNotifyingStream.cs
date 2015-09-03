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

    /// <summary>
    /// Bandwidth notifying stream.
    /// </summary>
    public class BandwidthNotifyingStream : NotifyPropertyChangedStream {
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
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Streams.BandwidthNotifyingStream"/> class.
        /// </summary>
        /// <param name="s">Stream which should be observered.</param>
        public BandwidthNotifyingStream(Stream s) : base(s) {
            this.blockingDetectionTimer = new Timer(2000);
            this.blockingDetectionTimer.Elapsed += delegate(object sender, ElapsedEventArgs args) {
                this.BitsPerSecond = (long)((this.bytesTransmittedSinceLastSecond * 8) / this.blockingDetectionTimer.Interval);
                this.bytesTransmittedSinceLastSecond = 0;
            };
            this.blockingDetectionTimer.Start();
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
            int result = this.Stream.Read(buffer, offset, count);
            this.CalculateBandwidth(result);
            return result;
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
        }

        /// <summary>
        /// Close this instance and calculates the bandwidth of the last second.
        /// </summary>
        public override void Close() {
            this.BitsPerSecond = CalcBitsPerSecond(this.start, DateTime.Now.AddMilliseconds(1), this.bytesTransmittedSinceLastSecond);
            this.blockingDetectionTimer.Stop();
            base.Close();
        }

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
        private static long CalcBitsPerSecond(DateTime start, DateTime end, long bytes) {
            if (end < start) {
                throw new ArgumentException("The end of a transmission must be higher than the start");
            }

            TimeSpan difference = end - start;
            double seconds = difference.TotalMilliseconds / 1000d;
            double dbytes = bytes;
            return (long)((dbytes * 8) / seconds);
        }

        /// <summary>
        /// Calculates the bandwidth.
        /// </summary>
        /// <param name='transmittedBytes'>
        /// Transmitted bytes.
        /// </param>
        private void CalculateBandwidth(int transmittedBytes) {
            this.bytesTransmittedSinceLastSecond += transmittedBytes;
            var diff = DateTime.Now - this.start;
            if (diff.Seconds >= 1) {
                this.BitsPerSecond = CalcBitsPerSecond(this.start, DateTime.Now, this.bytesTransmittedSinceLastSecond);
                this.bytesTransmittedSinceLastSecond = 0;
                this.start += diff;
                this.blockingDetectionTimer.Stop();
                this.blockingDetectionTimer.Start();
            }
        }
    }
}