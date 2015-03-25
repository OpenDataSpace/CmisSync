//-----------------------------------------------------------------------
// <copyright file="PausableStream.cs" company="GRAU DATA AG">
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
    using System.Threading;

    public class PausableStream : StreamWrapper {
        private ManualResetEvent waitHandle = new ManualResetEvent(false);

        public PausableStream(Stream s) : base(s) {
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

            // Pause here
            waitHandle.WaitOne();
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
            // Pause here
            waitHandle.WaitOne();
            return this.Stream.Read(buffer, offset, count);
        }

        public void Pause() {
            waitHandle.Reset();
        }

        public void Resume() {
            waitHandle.Set();
        }
    }
}