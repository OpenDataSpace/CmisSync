//-----------------------------------------------------------------------
// <copyright file="NonClosingHashStream.cs" company="GRAU DATA AG">
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
    using System.Security.Cryptography;

    /// <summary>
    /// This stream can be used like a CryptoStream, but does not closes/finilizes the given HashAlgorithm on dispose.
    /// Also any other operation than READ/WRITE are directly passed to the given stream.
    /// </summary>
    public class NonClosingHashStream : StreamWrapper {
        /// <summary>
        /// The used hash algorithm.
        /// </summary>
        private HashAlgorithm hashAlg;

        /// <summary>
        /// The used access mode. Read or write.
        /// </summary>
        private CryptoStreamMode mode;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Streams.NonClosingHashStream"/> class.
        /// </summary>
        /// <param name='stream'>
        /// Wrapped stream.
        /// </param>
        /// <param name='hashAlg'>
        /// Hash algorithm, which should be used for hash calculations.
        /// </param>
        /// <param name='mode'>
        /// Setting the mode, when hashing should be executed. On Read will transform the hash while reading, or Write mode for transforming while writing.
        /// </param>
        public NonClosingHashStream(Stream stream, HashAlgorithm hashAlg, CryptoStreamMode mode) : base(stream) {
            if (hashAlg == null) {
                throw new ArgumentNullException("Given hash algorithm must not be null");
            }

            this.hashAlg = hashAlg;
            this.mode = mode;
        }

        /// <summary>
        /// Gets the cipher mode.
        /// If CryptoStreamMode.Read is returned, only reading operations are used for hash calculation.
        /// If CryptoStreamMode.Write is returned, only writing operations are used for hash calculation.
        /// </summary>
        /// <value>
        /// The cipher mode.
        /// </value>
        public CryptoStreamMode CipherMode {
            get { return this.mode; }
        }

        /// <summary>
        /// Passes the call to exact the same method of the given stream.
        /// If mode is set to Read, the given HashAlgorithm is transformed by passing the output of the read operation.
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
        /// <returns>Count of read bytes</returns>
        public override int Read(byte[] buffer, int offset, int count) {
            int result = base.Read(buffer, offset, count);
            if (this.mode == CryptoStreamMode.Read) {
                this.hashAlg.TransformBlock(buffer, offset, result, buffer, offset);
            }

            return result;
        }

        /// <summary>
        /// Passes the call to exact the same method of the given stream.
        /// If mode is set to Write, the given HashAlgorithm is transformed by passing the input of the write operation.
        /// </summary>
        /// <param name='buffer'>
        /// Given Buffer.
        /// </param>
        /// <param name='offset'>
        /// Given Offset.
        /// </param>
        /// <param name='count'>
        /// Count of bytes.
        /// </param>
        public override void Write(byte[] buffer, int offset, int count) {
            if (this.mode == CryptoStreamMode.Write) {
                this.hashAlg.TransformBlock(buffer, offset, count, buffer, offset);
            }

            base.Write(buffer, offset, count);
        }
    }
}