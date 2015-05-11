//-----------------------------------------------------------------------
// <copyright file="ChunkedStream.cs" company="GRAU DATA AG">
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

    /// <summary>
    /// Chunked stream.
    /// </summary>
    public class ChunkedStream : Stream {
        private long chunkPosition;
        private Stream source;
        private long chunkSize;
        private long position;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.ChunkedStream"/> class.
        /// </summary>
        /// <param name="stream">Stream to chunk.</param>
        /// <param name="chunk">The chunksize.</param>
        public ChunkedStream(Stream stream, long chunk) {
            this.source = stream;
            this.chunkSize = chunk;
  ////if (!source.CanRead)
            ////{
            ////    throw new System.NotSupportedException("Read access is needed for ChunkedStream");
            ////}
        }

        /// <summary>
        /// Gets a value indicating whether this and the source stream can be read.
        /// </summary>
        /// <value><c>true</c> if this instance can read; otherwise, <c>false</c>.</value>
        public override bool CanRead { get
            { return this.source.CanRead; }
        }

        /// <summary>
        /// Gets a value indicating whether this and the source stream can written.
        /// </summary>
        /// <value><c>true</c> if this instance can write; otherwise, <c>false</c>.</value>
        public override bool CanWrite { get
            { return this.source.CanWrite; }
        }

        /// <summary>
        /// Gets a value indicating whether this and the source stream are able to be seeked.
        /// </summary>
        /// <value><c>true</c> if this instance can seek; otherwise, <c>false</c>.</value>
        public override bool CanSeek {
            get { return this.source.CanSeek; }
        }

        /// <summary>
        /// Gets or sets the chunk position.
        /// </summary>
        /// <value>The chunk position.</value>
        public long ChunkPosition {
            get {
                return this.chunkPosition;
            }

            set {
                this.source.Position = value;
                this.chunkPosition = value;
            }
        }

        /// <summary>
        /// Gets the length of the actual chunk.
        /// </summary>
        /// <value>The length.</value>
        public override long Length {
            get {
                long lengthSource = this.source.Length;
                if (lengthSource <= this.ChunkPosition) {
                    return 0;
                }

                long length = lengthSource - this.ChunkPosition;
                if (length >= this.chunkSize) {
                    return this.chunkSize;
                } else {
                    return length;
                }
            }
        }

        /// <summary>
        /// Gets or sets the position in the chunk.
        /// </summary>
        /// <value>The position.</value>
        public override long Position {
            get {
                if (!this.CanSeek) {
                    return this.position;
                }

                long offset = this.source.Position - this.ChunkPosition;
                if (offset < 0 || offset > this.chunkSize) {
                    Debug.Assert(false, string.Format("Position {0} not in [0,{1}]", offset, this.chunkSize));
                }

                return offset;
            }

            set {
                if (value < 0 || value > this.chunkSize) {
                    throw new System.ArgumentOutOfRangeException(string.Format("Position {0} not in [0,{1}]", value, this.chunkSize));
                }

                this.source.Position = this.ChunkPosition + value;
            }
        }

        /// <summary>
        /// Flush all data of the source stream.
        /// </summary>
        public override void Flush() {
            this.source.Flush();
        }

        /// <summary>
        /// Read the specified buffer from the given offset and with the length of count.
        /// </summary>
        /// <param name="buffer">The buffer to read.</param>
        /// <param name="offset">Offset to start reading.</param>
        /// <param name="count">Count of bytes.</param>
        /// <returns>
        /// bytes read
        /// </returns>
        public override int Read(byte[] buffer, int offset, int count) {
            if (offset < 0) {
                throw new System.ArgumentOutOfRangeException("offset", offset, "offset is negative");
            }

            if (count < 0) {
                throw new System.ArgumentOutOfRangeException("count", count, "count is negative");
            }

            if (count > this.chunkSize - this.Position) {
                count = (int)(this.chunkSize - this.Position);
            }

            count = this.source.Read(buffer, offset, count);
            this.position += count;
            return count;
        }

        /// <summary>
        /// Write the specified buffer from the given offset and the count.
        /// </summary>
        /// <param name="buffer">The buffer to write.</param>
        /// <param name="offset">Offset to start writing.</param>
        /// <param name="count">Count of bytes.</param>
        public override void Write(byte[] buffer, int offset, int count) {
            if (offset < 0) {
                throw new System.ArgumentOutOfRangeException("offset", offset, "offset is negative");
            }

            if (count < 0) {
                throw new System.ArgumentOutOfRangeException("count", count, "count is negative");
            }

            if (count > this.chunkSize - this.Position) {
                throw new System.ArgumentOutOfRangeException("count", count, "count is overflow");
            }

            this.source.Write(buffer, offset, count);
            this.position += count;
        }

        /// <summary>
        /// Seek the specified offset and origin.
        /// </summary>
        /// <param name="offset">The Offset.</param>
        /// <param name="origin">The Origin.</param>
        /// <returns>the found position</returns>
        public override long Seek(long offset, SeekOrigin origin) {
            Debug.Assert(false, "TODO");
            return this.source.Seek(offset, origin);
        }

        /// <summary>
        /// Sets the length. Is not implemented at correctly. It simply passes the call to the source stream.
        /// </summary>
        /// <param name="value">The length to set.</param>
        public override void SetLength(long value) {
            Debug.Assert(false, "TODO");
            this.source.SetLength(value);
        }
    }
}