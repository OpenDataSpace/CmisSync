//-----------------------------------------------------------------------
// <copyright file="UploadFailedException.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.ContentTasks
{
    using System;
    using System.Runtime.Serialization;

    using DotCMIS.Client;

    /// <summary>
    /// Upload failed exception.
    /// </summary>
    [Serializable]
    public class UploadFailedException : Exception
    {
        private IDocument doc;

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadFailedException"/> class.
        /// </summary>
        /// <param name='inner'>
        /// Inner exception cause the upload failure.
        /// </param>
        /// <param name='lastSuccessfulDocumentState'>
        /// Last successful uploaded document state.
        /// </param>
        public UploadFailedException(Exception inner, IDocument lastSuccessfulDocumentState) : base("Upload Failed", inner)
        {
            this.doc = lastSuccessfulDocumentState;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.ContentTasks.UploadFailedException"/> class.
        /// </summary>
        public UploadFailedException() : this("Upload Failed")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.ContentTasks.UploadFailedException"/> class.
        /// </summary>
        /// <param name="message">Upload failing reason message.</param>
        public UploadFailedException(string message) : base(message)
        {
            this.doc = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.ContentTasks.UploadFailedException"/> class.
        /// </summary>
        /// <param name="message">Upload failing reason message.</param>
        /// <param name="innerException">Inner exception.</param>
        public UploadFailedException(string message, Exception innerException) : base(message, innerException)
        {
            this.doc = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.ContentTasks.UploadFailedException"/> class.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected UploadFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.doc = null;
        }

        /// <summary>
        /// Gets the last successful uploaded document state.
        /// </summary>
        /// <value>
        /// The last successful uploaded document state.
        /// </value>
        public IDocument LastSuccessfulDocument 
        {
            get
            {
                return this.doc;
            }
        }
    }
}
