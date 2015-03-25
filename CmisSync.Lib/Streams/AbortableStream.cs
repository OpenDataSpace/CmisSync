//-----------------------------------------------------------------------
// <copyright file="AbortableStream.cs" company="GRAU DATA AG">
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

    public class AbortableStream : StreamWrapper {
        private bool aborted = false;
        private object l = new object();
        public AbortableStream(Stream s) : base(s) {
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
            lock(this.l) {
                if (this.aborted) {
                    throw new FileTransmission.AbortException();
                }
            }

            return this.Stream.Read(buffer, offset, count);
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

            lock(this.l) {
                if (this.aborted) {
                    throw new FileTransmission.AbortException();
                }
            }
        }

        /// <summary>
        /// Abort this instance.
        /// </summary>
        public void Abort() {
            lock(this.l) {
                this.aborted = true;
            }
        }
    }
}