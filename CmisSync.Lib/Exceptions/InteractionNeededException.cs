//-----------------------------------------------------------------------
// <copyright file="InteractionNeededException.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Exceptions {
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Exceptions;

    /// <summary>
    /// Interaction needed exception. This exception should be thrown if a user must be informed about a needed interaction to solve a problem/conflict.
    /// </summary>
    [Serializable]
    public class InteractionNeededException : AbstractInteractionNeededException {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Exceptions.InteractionNeededException"/> class.
        /// </summary>
        public InteractionNeededException() : base() {
            this.Level = ExceptionLevel.Info;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Exceptions.InteractionNeededException"/> class.
        /// </summary>
        /// <param name="msg">Exception message.</param>
        public InteractionNeededException(string msg) : base(msg) {
            this.Level = ExceptionLevel.Info;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Exceptions.InteractionNeededException"/> class.
        /// </summary>
        /// <param name="msg">Exception message.</param>
        /// <param name="inner">Inner exception.</param>
        public InteractionNeededException(string msg, Exception inner) : base(msg, inner) {
            this.Level = ExceptionLevel.Info;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Exceptions.InteractionNeededException"/> class.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected InteractionNeededException(SerializationInfo info, StreamingContext context) : base(info, context) {
            this.Level = ExceptionLevel.Info;
        }
    }
}