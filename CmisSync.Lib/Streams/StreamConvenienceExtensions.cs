//-----------------------------------------------------------------------
// <copyright file="StreamConvenienceExtensions.cs" company="GRAU DATA AG">
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
    /// Stream convenience extensions.
    /// </summary>
    public static class StreamConvenienceExtensions {
        /// <summary>
        /// Copies stream content to output stream until given byte count is reached.
        /// </summary>
        /// <param name="input">Input stream.</param>
        /// <param name="output">Output stream.</param>
        /// <param name="bufferSize">Buffer size.</param>
        /// <param name="bytes">Bytes to be copied from input stream to output stream.</param>
        public static void CopyTo(this Stream input, Stream output, int bufferSize, int bytes) {
            if (output == null) {
                throw new ArgumentNullException("output");
            }

            byte[] buffer = new byte[bufferSize];
            int read;
            while (bytes > 0 && (read = input.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0) {
                output.Write(buffer, 0, read);
                bytes -= read;
            }
        }
    }
}