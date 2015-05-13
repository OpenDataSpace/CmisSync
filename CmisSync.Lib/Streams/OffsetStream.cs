//-----------------------------------------------------------------------
// <copyright file="OffsetStream.cs" company="GRAU DATA AG">
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

    /// <summary>
    /// Offset stream provides the possibility to simulate a longer stream with a virtual empty space at the beginning with the size of offset.
    /// </summary>
    public class OffsetStream : StreamWrapper {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Streams.OffsetStream"/> class.
        /// </summary>
        /// <param name='stream'>
        /// Stream which should be used after the given offset.
        /// </param>
        /// <param name='offset'>
        /// Size of the empty offset.
        /// </param>
        public OffsetStream(Stream stream, long offset = 0) : base(stream) {
            if (offset < 0) {
                throw new ArgumentOutOfRangeException("A negative offset is forbidden");
            }

            this.Offset = offset;
        }

        /// <summary>
        /// Gets the offset (virtual empty size)
        /// </summary>
        /// <value>
        /// The offset.
        /// </value>
        public long Offset { get; private set; }

        #region overrideCode
        /// <summary>
        /// Gets the length of the virtual stream.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public override long Length {
            get {
                return this.Stream.Length + this.Offset;
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
                return this.Stream.Position + this.Offset;
            }

            set {
                if (value < this.Offset) {
                    throw new ArgumentOutOfRangeException("given position is out of range");
                }

                this.Stream.Position = value - this.Offset;
            }
        }

        /// <summary>
        /// Seek the specified offset based on the given origin position.
        /// </summary>
        /// <param name='offset'>
        /// Offset.
        /// </param>
        /// <param name='origin'>
        /// Origin.
        /// </param>
        public override long Seek(long offset, SeekOrigin origin) {
            return this.Stream.Seek(offset, origin) + this.Offset;
        }

        /// <summary>
        /// Sets the length to the given length. It must be greater than the Offset
        /// </summary>
        /// <param name='length'>
        /// The new Length
        /// </param>
        public override void SetLength(long length) {
            if (length < this.Offset) {
                throw new ArgumentOutOfRangeException(string.Format("Given length {0} is smaller than Offset {1}", length, this.Offset));
            }

            this.Stream.SetLength(length - this.Offset);
        }
        #endregion
    }
}