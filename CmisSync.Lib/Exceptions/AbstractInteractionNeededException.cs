//-----------------------------------------------------------------------
// <copyright file="AbstractInteractionNeededException.cs" company="GRAU DATA AG">
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
    /// Abstract Interaction needed exception. This exception should be thrown if a user must be informed about a needed interaction to solve a problem/conflict.
    /// </summary>
    [Serializable]
    public abstract class AbstractInteractionNeededException : Exception {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Exceptions.AbstractInteractionNeededException"/> class.
        /// </summary>
        public AbstractInteractionNeededException() : base() {
            this.InitParams();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Exceptions.AbstractInteractionNeededException"/> class.
        /// </summary>
        /// <param name="msg">Exception message.</param>
        public AbstractInteractionNeededException(string msg) : base(msg) {
            this.InitParams();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Exceptions.AbstractInteractionNeededException"/> class.
        /// </summary>
        /// <param name="msg">Exception message.</param>
        /// <param name="inner">Inner exception.</param>
        public AbstractInteractionNeededException(string msg, Exception inner) : base(msg, inner) {
            this.InitParams();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Exceptions.AbstractInteractionNeededException"/> class.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected AbstractInteractionNeededException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the details. The details are technical details, such as error messages from server or a stack trace.
        /// </summary>
        /// <value>The details of the problem.</value>
        public string Details { get; set; }

        /// <summary>
        /// Gets or sets the exception level.
        /// </summary>
        /// <value>The level.</value>
        public ExceptionLevel Level { get; protected set; }

        /// <summary>
        /// Gets the actions, which can be executed to solve the problem.
        /// </summary>
        /// <value>The actions.</value>
        public Dictionary<string, Action> Actions { get; private set; }

        /// <summary>
        /// Gets the affected files. If local files or folders are involved to the problem, they should be listed.
        /// </summary>
        /// <value>The affected files.</value>
        public List<IFileSystemInfo> AffectedFiles { get; private set; }

        private void InitParams() {
            this.AffectedFiles = new List<IFileSystemInfo>();
            this.Actions = new Dictionary<string, Action>();
            this.Title = this.GetType().Name;
            this.Description = this.Message;
            this.Details = string.Empty;
            if (this.InnerException is CmisBaseException) {
                this.Details = (this.InnerException as CmisBaseException).ErrorContent;
            } else if (this.InnerException != null) {
                this.Details = this.InnerException.StackTrace ?? string.Empty;
            }
        }
    }
}