//-----------------------------------------------------------------------
// <copyright file="ExtendedAttributeException.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Storage.FileSystem
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;

    /// <summary>
    /// Extended attribute exception.
    /// </summary>
    [Serializable]
    public class ExtendedAttributeException : IOException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Storage.FileSystem.ExtendedAttributeException"/> class.
        /// </summary>
        public ExtendedAttributeException() : base("ExtendedAttribute manipulation exception") {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Storage.FileSystem.ExtendedAttributeException"/> class.
        /// </summary>
        /// <param name="msg">Eception message.</param>
        public ExtendedAttributeException(string msg) : base(msg) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Storage.FileSystem.ExtendedAttributeException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="inner">Inner exception.</param>
        public ExtendedAttributeException(string message, Exception inner) : base(message, inner) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Storage.FileSystem.ExtendedAttributeException"/> class.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected ExtendedAttributeException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}