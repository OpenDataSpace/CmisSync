//-----------------------------------------------------------------------
// <copyright file="RepositoryExceptionEventArgs.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Cmis {
    using System;

    using CmisSync.Lib.Exceptions;

    /// <summary>
    /// Exception type.
    /// </summary>
    public enum ExceptionType {
        /// <summary>
        /// Unknown Exception Type
        /// </summary>
        Unknown,

        /// <summary>
        /// The local sync target folder is deleted.
        /// </summary>
        LocalSyncTargetDeleted,

        /// <summary>
        /// The file upload is blocked due to virus detection on the server.
        /// </summary>
        FileUploadBlockedDueToVirusDetected
    }

    /// <summary>
    /// Repository exception event arguments.
    /// </summary>
    public class RepositoryExceptionEventArgs : EventArgs {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Cmis.RepositoryExceptionEventArgs"/> class.
        /// </summary>
        /// <param name="level">Exception Level.</param>
        /// <param name="type">Exception Type.</param>
        /// <param name="e">Original exception.</param>
        public RepositoryExceptionEventArgs(ExceptionLevel level, ExceptionType type, Exception e = null) {
            this.Level = level;
            this.Exception = e;
            this.Type = type;
        }

        /// <summary>
        /// Gets the type of the exception.
        /// </summary>
        /// <value>The type.</value>
        public ExceptionType Type { get; private set; }

        /// <summary>
        /// Gets the level of the exception.
        /// </summary>
        /// <value>The level.</value>
        public ExceptionLevel Level { get; private set; }

        /// <summary>
        /// Gets the original exception.
        /// </summary>
        /// <value>The exception.</value>
        public Exception Exception { get; private set; }
    }
}