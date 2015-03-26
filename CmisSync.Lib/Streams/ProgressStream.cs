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

    /// <summary>
    /// Progress reporting stream.
    /// </summary>
    public class ProgressStream : NotifyPropertyChangedStream {
        /// <summary>
        /// The length of the underlaying stream.
        /// </summary>
        private long length = -1;

        /// <summary>
        /// The position of the underlaying stream.
        /// </summary>
        private long position = -1;

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
        public ProgressStream(Stream stream) : base(stream) {
            try {
                this.length = stream.Length;
            } catch (NotSupportedException) {
            }
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
                    this.NotifyPropertyChanged((Utils.NameOf(() => this.Length)));
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
                if (pos != this.position) {
                    this.position = pos;
                    this.NotifyPropertyChanged(Utils.NameOf(() => this.Position));
                }

                return pos;
            }

            set {
                this.Stream.Position = value;
                if (this.position != value) {
                    this.position = value;
                    this.NotifyPropertyChanged(Utils.NameOf(() => this.Position));
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
                if (this.length < 0 || this.position < 0) {
                    return null;
                }

                if (this.Length == 0) {
                    return 100d;
                }

                return Math.Round(((double)this.Position * 100d) / (double)this.Length, 1);
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
            long pos = this.Stream.Position;
            if (pos != this.position) {
                this.position = pos;
                this.NotifyPropertyChanged(Utils.NameOf(() => this.Position));
            }

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
            int result = this.Stream.Read(buffer, offset, count);
            this.position += result;
            this.NotifyPropertyChanged(Utils.NameOf(() => this.Position));
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
            if (this.length != value) {
                this.length = value;
                this.NotifyPropertyChanged(Utils.NameOf(() => this.Length));
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
            this.position += count;
            this.NotifyPropertyChanged(Utils.NameOf(() => this.Position));
        }
#endregion
    }
}