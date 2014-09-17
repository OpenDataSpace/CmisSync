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

namespace CmisSync.Lib.Streams
{
    using System;
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// Bandwidth limited stream.
    /// </summary>
    public class BandwidthLimitedStream : StreamWrapper
    {
        /// <summary>
        /// Locks the limit manipulation to prevent concurrent accesses
        /// </summary>
        private object limitLock = new object();

        /// <summary>
        /// The Limit of bytes which could be read per second. The limit is disabled if set to -1.
        /// </summary>
        private long readLimit = -1;

        /// <summary>
        /// The Limit of bytes which could be written per second. The limit is disabled if set to -1.
        /// </summary>
        private long writeLimit = -1;
        private Stopwatch ReadWatch;
        private Stopwatch WriteWatch;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Streams.BandwidthLimitedStream"/> class.
        /// </summary>
        /// <param name='s'>
        /// The stream instance, which should be limited.
        /// </param>
        public BandwidthLimitedStream(Stream s) : base(s)
        {
            this.Init();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Streams.BandwidthLimitedStream"/> class.
        /// </summary>
        /// <param name='s'>
        /// The stream instance, which should be limited.
        /// </param>
        /// <param name='limit'>
        /// Limit.
        /// </param>
        public BandwidthLimitedStream(Stream s, long limit) : base(s)
        {
            this.Init();
            ReadLimit = limit;
            WriteLimit = limit;
        }

        /// <summary>
        /// Gets or sets the read limit.
        /// </summary>
        /// <value>
        /// The read limit.
        /// </value>
        public long ReadLimit
        {
            get
            {
                return this.readLimit; 
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Limit cannot be negative");
                }

                lock (this.limitLock)
                {
                    this.readLimit = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the write limit.
        /// </summary>
        /// <value>
        /// The write limit.
        /// </value>
        public long WriteLimit
        {
            get
            {
                return this.writeLimit;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Limit cannot be negative");
                }

                lock (this.limitLock)
                {
                    this.writeLimit = value;
                }
            }
        }

        /// <summary>
        /// Disables the limits.
        /// </summary>
        public void DisableLimits()
        {
            lock (this.limitLock)
            {
                this.readLimit = -1;
                this.writeLimit = -1;
            }
        }

        /// <summary>
        /// Disables the read limit.
        /// </summary>
        public void DisableReadLimit()
        {
            lock (this.limitLock)
            {
                this.readLimit = -1;
            }
        }

        /// <summary>
        /// Disables the write limit.
        /// </summary>
        public void DisableWriteLimit()
        {
            lock (this.limitLock)
            {
                this.writeLimit = -1;
            }
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
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (this.readLimit < 0)
            {
                return Stream.Read(buffer, offset, count);
            }
            else
            {
                // TODO Sleep must be implemented
                return Stream.Read(buffer, offset, count);
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
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (this.WriteLimit < 0)
            {
                this.Stream.Write(buffer, offset, count);
            }
            else
            {
                // TODO Sleep must be implemented
                this.Stream.Write(buffer, offset, count);
            }
        }

        /// <summary>
        /// Init this instance.
        /// </summary>
        private void Init()
        {
            this.WriteWatch = new Stopwatch();
            this.ReadWatch = new Stopwatch();
        }
    }
}
