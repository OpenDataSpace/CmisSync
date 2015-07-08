//-----------------------------------------------------------------------
// <copyright file="BandwidthLimitedStream.cs" company="GRAU DATA AG">
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
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    /// <summary>
    /// Bandwidth limited stream.
    /// </summary>
    public class BandwidthLimitedStream : NotifyPropertyChangedStream {
        /// <summary>
        /// Locks the limit manipulation to prevent concurrent accesses
        /// </summary>
        private object limitLock = new object();

        /// <summary>
        /// The Limit of bytes which could be read per second. The limit is disabled if set to -1.
        /// </summary>
        private long? readLimit = null;

        /// <summary>
        /// The Limit of bytes which could be written per second. The limit is disabled if set to -1.
        /// </summary>
        private long? writeLimit = null;
        private int readCount = 0;
        private int writeCount = 0;
        private DateTime readTimeStamp = DateTime.Now;
        private DateTime writeTimeStamp = DateTime.Now;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Streams.BandwidthLimitedStream"/> class.
        /// </summary>
        /// <param name='s'>
        /// The stream instance, which should be limited.
        /// </param>
        public BandwidthLimitedStream(Stream s) : base(s) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Streams.BandwidthLimitedStream"/> class.
        /// </summary>
        /// <param name='s'>
        /// The stream instance, which should be limited.
        /// </param>
        /// <param name='limit'>
        /// Up and download limit.
        /// </param>
        public BandwidthLimitedStream(Stream s, long limit) : this(s) {
            this.ReadLimit = limit;
            this.WriteLimit = limit;
        }

        /// <summary>
        /// Gets or sets the read limit.
        /// </summary>
        /// <value>
        /// The read limit.
        /// </value>
        public long? ReadLimit {
            get {
                return this.readLimit;
            }

            set {
                if (value != null && value <= 0) {
                    throw new ArgumentException("Limit cannot be negative");
                }

                lock (this.limitLock) {
                    if (value != this.readLimit) {
                        this.readLimit = value;
                        this.NotifyPropertyChanged(Utils.NameOf(() => this.ReadLimit));
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the write limit.
        /// </summary>
        /// <value>
        /// The write limit.
        /// </value>
        public long? WriteLimit {
            get {
                return this.writeLimit;
            }

            set {
                if (value != null && value <= 0) {
                    throw new ArgumentException("Limit cannot be negative");
                }

                lock (this.limitLock) {
                    if (value != this.writeLimit) {
                        this.writeLimit = value;
                        this.NotifyPropertyChanged(Utils.NameOf(() => this.WriteLimit));
                    }
                }
            }
        }

        /// <summary>
        /// Disables the limits.
        /// </summary>
        public void DisableLimits() {
            this.DisableReadLimit();
            this.DisableWriteLimit();
        }

        /// <summary>
        /// Disables the read limit.
        /// </summary>
        public void DisableReadLimit() {
            this.ReadLimit = null;
        }

        /// <summary>
        /// Disables the write limit.
        /// </summary>
        public void DisableWriteLimit() {
            this.WriteLimit = null;
        }

        /// <summary>
        /// Read the specified buffer, offset and count.
        /// </summary>
        /// <param name='buffer'>
        /// Target buffer.
        /// </param>
        /// <param name='offset'>
        /// Offset.
        /// </param>
        /// <param name='count'>
        /// Count of bytes.
        /// </param>
        /// <returns>The count of read bytes.</returns>
        public override int Read(byte[] buffer, int offset, int count) {
            if (this.ReadLimit == null) {
                return Stream.Read(buffer, offset, count);
            } else {
                var maxBytes = GetCountLimit(ref this.readTimeStamp, ref this.readCount, (long)this.readLimit);
                count = Math.Min(count, maxBytes);
                int result = Stream.Read(buffer, offset, count);
                this.readCount += result;
                return result;
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
            if (this.WriteLimit == null) {
                this.Stream.Write(buffer, offset, count);
            } else {
                int localOffset = 0;
                var maxBytes = GetCountLimit(ref this.writeTimeStamp, ref this.writeCount, (long)this.writeLimit);
                while (maxBytes < count) {
                    this.Stream.Write(buffer, offset + localOffset, maxBytes);
                    count -= maxBytes;
                    localOffset += maxBytes;
                    this.writeCount += maxBytes;
                    maxBytes = GetCountLimit(ref this.writeTimeStamp, ref this.writeCount, (long)this.writeLimit);
                }

                this.Stream.Write(buffer, offset + localOffset, count);
                this.writeCount += count;
            }
        }

        private static int GetCountLimit(ref DateTime timestamp, ref int oldCount, long hardLimit) {
            var now = DateTime.Now;
            var difference = now - timestamp;
            if (difference.TotalMilliseconds <= 1000) {
                if (oldCount < hardLimit) {
                    return (int)hardLimit - oldCount;
                } else {
                    Thread.Sleep(1000 - difference.Milliseconds);
                    oldCount = 0;
                    timestamp = DateTime.Now;
                    return (int)hardLimit;
                }
            } else {
                return (int)hardLimit - oldCount;
            }

/*            if (oldCount < hardLimit) {
                Console.WriteLine("A");
                return (int)(hardLimit - oldCount);
            } else {
                Console.WriteLine("B");
                try {
                    Thread.Sleep((int)(1000 - watch.ElapsedMilliseconds));
                } catch (ThreadAbortException) {
                }

                oldCount = 0;
                return (int)hardLimit;
            }*/
        }
    }
}