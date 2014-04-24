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
using System;
using System.Diagnostics;
using System.IO;

namespace CmisSync.Lib
{
    /// <summary>
    /// Chunked stream.
    /// </summary>
    public class ChunkedStream : Stream
    {
        private Stream source;
        private long chunkSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.ChunkedStream"/> class.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <param name="chunk">Chunk.</param>
        public ChunkedStream(Stream stream, long chunk)
        {
            source = stream;
            chunkSize = chunk;

            //if (!source.CanRead)
            //{
            //    throw new System.NotSupportedException("Read access is needed for ChunkedStream");
            //}
        }

        /// <summary>
        /// Gets a value indicating whether this and the source stream can be read.
        /// </summary>
        /// <value><c>true</c> if this instance can read; otherwise, <c>false</c>.</value>
        public override bool CanRead { get { return source.CanRead; } }
        /// <summary>
        /// Gets a value indicating whether this and the source stream can written.
        /// </summary>
        /// <value><c>true</c> if this instance can write; otherwise, <c>false</c>.</value>
        public override bool CanWrite { get { return source.CanWrite; } }
        /// <summary>
        /// Gets a value indicating whether this and the source stream are able to be seeked.
        /// </summary>
        /// <value><c>true</c> if this instance can seek; otherwise, <c>false</c>.</value>
        public override bool CanSeek { get { return source.CanSeek; } }
        /// <summary>
        /// Flush all data of the source stream.
        /// </summary>
        public override void Flush() { source.Flush(); }

        private long chunkPosition;
        /// <summary>
        /// Gets or sets the chunk position.
        /// </summary>
        /// <value>The chunk position.</value>
        public long ChunkPosition
        {
            get
            {
                return chunkPosition;
            }

            set
            {
                source.Position = value;
                chunkPosition = value;
            }
        }

        /// <summary>
        /// Gets the length of the actual chunk.
        /// </summary>
        /// <value>The length.</value>
        public override long Length
        {
            get
            {
                long lengthSource = source.Length;
                if (lengthSource <= ChunkPosition)
                {
                    return 0;
                }

                long length = lengthSource - ChunkPosition;
                if (length >= chunkSize)
                {
                    return chunkSize;
                }
                else
                {
                    return length;
                }
            }
        }

        private long position;
        /// <summary>
        /// Gets or sets the position in the chunk.
        /// </summary>
        /// <value>The position.</value>
        public override long Position
        {
            get
            {
                if (!CanSeek)
                {
                    return position;
                }

                long offset = source.Position - ChunkPosition;
                if (offset < 0 || offset > chunkSize)
                {
                    Debug.Assert(false, String.Format("Position {0} not in [0,{1}]", offset, chunkSize));
                }
                return offset;
            }

            set
            {
                if (value < 0 || value > chunkSize)
                {
                    throw new System.ArgumentOutOfRangeException(String.Format("Position {0} not in [0,{1}]", value, chunkSize));
                }
                source.Position = ChunkPosition + value;
            }
        }
        /// <summary>
        /// Read the specified buffer from the given offset and with the length of count.
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="count">Count.</param>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (offset < 0)
            {
                throw new System.ArgumentOutOfRangeException("offset", offset, "offset is negative");
            }
            if (count < 0)
            {
                throw new System.ArgumentOutOfRangeException("count", count, "count is negative");
            }

            if (count > chunkSize - Position)
            {
                count = (int)(chunkSize - Position);
            }
            count = source.Read(buffer, offset, count);
            position += count;
            return count;
        }

        /// <summary>
        /// Write the specified buffer from the given offset and the count.
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="count">Count.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (offset < 0)
            {
                throw new System.ArgumentOutOfRangeException("offset", offset, "offset is negative");
            }
            if (count < 0)
            {
                throw new System.ArgumentOutOfRangeException("count", count, "count is negative");
            }

            if (count > chunkSize - Position)
            {
                throw new System.ArgumentOutOfRangeException("count", count, "count is overflow");
            }
            source.Write(buffer, offset, count);
            position += count;
        }

        /// <summary>
        /// Seek the specified offset and origin.
        /// </summary>
        /// <param name="offset">Offset.</param>
        /// <param name="origin">Origin.</param>
        public override long Seek(long offset, SeekOrigin origin)
        {
            Debug.Assert(false, "TODO");
            return source.Seek(offset, origin);
        }
        /// <summary>
        /// Sets the length. Is not implemented at correctly. It simply passes the call to the source stream.
        /// </summary>
        /// <param name="value">Value.</param>
        public override void SetLength(long value)
        {
            Debug.Assert(false, "TODO");
            source.SetLength(value);
        }
    }
}
