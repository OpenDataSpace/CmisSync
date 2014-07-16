//-----------------------------------------------------------------------
// <copyright file="AbortException.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.FileTransmission
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// Abort exception.
    /// </summary>
    [Serializable]
    public class AbortException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.FileTransmission.AbortException"/> class.
        /// </summary>
        public AbortException() : base("Abort exception")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.FileTransmission.AbortException"/> class.
        /// </summary>
        /// <param name="msg">Abortion message.</param>
        public AbortException(string msg) : base(msg)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.FileTransmission.AbortException"/> class.
        /// </summary>
        /// <param name="message">Abortion Message.</param>
        /// <param name="inner">Inner exception.</param>
        public AbortException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.FileTransmission.AbortException"/> class.
        /// </summary>
        /// <param name="info">Serializaction info.</param>
        /// <param name="context">Streaming context.</param>
        protected AbortException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
