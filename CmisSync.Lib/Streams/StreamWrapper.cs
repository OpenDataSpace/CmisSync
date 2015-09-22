//-----------------------------------------------------------------------
// <copyright file="StreamWrapper.cs" company="GRAU DATA AG">
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
    /// The stream wrapper class is used to hide a given stream and provide a wrapper to be
    /// overridden by other stream extending classes without the need of the implementing class
    /// to implement each function of the Stream interface
    /// </summary>
    public class StreamWrapper : Stream {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Streams.StreamWrapper"/> class.
        /// Takes a stream and wrapps all calls.
        /// </summary>
        /// <param name='stream'>
        /// Stream to be wrapped.
        /// </param>
        public StreamWrapper(Stream stream) {
            if (stream == null) {
                throw new ArgumentNullException("stream");
            }

            this.Stream = stream;
        }

        #region wrapperCode
        /// <summary>
        /// Gets a value indicating whether the wrapped instance can read.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can read; otherwise, <c>false</c>.
        /// </value>
        public override bool CanRead {
            get {
                return Stream.CanRead;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the wrapped instance can seek.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can seek; otherwise, <c>false</c>.
        /// </value>
        public override bool CanSeek {
            get {
                return this.Stream.CanSeek;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the wrapped instance can write.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can write; otherwise, <c>false</c>.
        /// </value>
        public override bool CanWrite {
            get {
                return this.Stream.CanWrite;
            }
        }

        /// <summary>
        /// Gets the length of the the wrapped instance.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public override long Length {
            get {
                return this.Stream.Length;
            }
        }

        /// <summary>
        /// Gets or sets the position of the wrapped instance.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public override long Position {
            get {
                return this.Stream.Position;
            }

            set {
                this.Stream.Position = value;
            }
        }

        /// <summary>
        /// Gets or sets the stream.
        /// </summary>
        /// <value>
        /// The stream.
        /// </value>
        protected Stream Stream { get; set; }

        /// <summary>
        /// Flush the wrapped instance.
        /// </summary>
        public override void Flush() {
            this.Stream.Flush();
        }

        /// <summary>
        /// Begins the read on the wrapped instance.
        /// </summary>
        /// <returns>
        /// The result of the the wrapped instance call.
        /// </returns>
        /// <param name='buffer'>
        /// Buffer passed to the wrapped instance.
        /// </param>
        /// <param name='offset'>
        /// Offset passed to the wrapped instance.
        /// </param>
        /// <param name='count'>
        /// Count passed to the wrapped instance.
        /// </param>
        /// <param name='callback'>
        /// Callback passed to the wrapped instance.
        /// </param>
        /// <param name='state'>
        /// State passed to the wrapped instance.
        /// </param>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) {
            return this.Stream.BeginRead(buffer, offset, count, callback, state);
        }

        /// <summary>
        /// Seeks on the wrapped instance with the specified offset and origin.
        /// </summary>
        /// <param name='offset'>
        /// Offset passed to the wrapped instance.
        /// </param>
        /// <param name='origin'>
        /// Origin passed to the wrapped instance.
        /// </param>
        /// <returns>
        /// The result of the call passed to the wrapped instance.
        /// </returns>
        public override long Seek(long offset, SeekOrigin origin) {
            return this.Stream.Seek(offset, origin);
        }

        /// <summary>
        /// Read the specified buffer, offset and count.
        /// </summary>
        /// <param name='buffer'>
        /// Buffer passed to the wrapped instance.
        /// </param>
        /// <param name='offset'>
        /// Offset passed to the wrapped instance.
        /// </param>
        /// <param name='count'>
        /// Count passed to the wrapped instance.
        /// </param>
        /// <returns>
        /// The result of the call passed to the wrapped instance.
        /// </returns>
        public override int Read(byte[] buffer, int offset, int count) {
            return this.Stream.Read(buffer, offset, count);
        }

        /// <summary>
        /// Sets the length of the wrapped instance.
        /// </summary>
        /// <param name='value'>
        /// Value passed to the wrapped instance.
        /// </param>
        public override void SetLength(long value) {
            this.Stream.SetLength(value);
        }

        /// <summary>
        /// Write the specified buffer, offset and count.
        /// </summary>
        /// <param name='buffer'>
        /// Buffer passed to the wrapped instance.
        /// </param>
        /// <param name='offset'>
        /// Offset passed to the wrapped instance.
        /// </param>
        /// <param name='count'>
        /// Count passed to the wrapped instance.
        /// </param>
        public override void Write(byte[] buffer, int offset, int count) {
            this.Stream.Write(buffer, offset, count);
        }
        #endregion
    }
}