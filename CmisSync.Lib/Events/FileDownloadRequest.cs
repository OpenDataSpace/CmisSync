//-----------------------------------------------------------------------
// <copyright file="FileDownloadRequest.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Events
{
    using System;
    using System.IO;

    using DotCMIS.Client;

    /// <summary>
    /// File download request.
    /// </summary>
    public class FileDownloadRequest : ISyncEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.FileDownloadRequest"/> class.
        /// </summary>
        /// <param name='doc'>
        /// The requested Document.
        /// </param>
        /// <param name='localPath'>
        /// The Local path.
        /// </param>
        public FileDownloadRequest(IDocument doc, string localPath)
        {
            if (doc == null)
            {
                throw new ArgumentNullException("The document object which should be downloaded must not be null");
            }
                
            if (localPath == null)
            {
                throw new ArgumentNullException(string.Format("The target directory path where the document \"{0}\" should be saved cannot be null", doc.Name));
            }
                
            this.Document = doc;
            this.LocalPath = localPath;
        }

        /// <summary>
        /// Gets the document.
        /// </summary>
        /// <value>
        /// The document.
        /// </value>
        public IDocument Document { get; private set; }

        /// <summary>
        /// Gets the local path.
        /// </summary>
        /// <value>
        /// The local path.
        /// </value>
        public string LocalPath { get; private set; }

        /// <summary>
        /// Gets the target file path.
        /// </summary>
        /// <value>
        /// The target file path.
        /// </value>
        public string TargetFilePath
        {
            get
            {
                return Path.Combine(this.LocalPath, this.Document.Name);
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="CmisSync.Lib.Events.FileDownloadRequest"/>.
        /// </summary>
        /// <param name='obj'>
        /// The <see cref="System.Object"/> to compare with the current <see cref="CmisSync.Lib.Events.FileDownloadRequest"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="CmisSync.Lib.Events.FileDownloadRequest"/>; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            FileDownloadRequest other = obj as FileDownloadRequest;
            if (other == null)
            {
                return false;
            }

            if (other.Document.Equals(this.Document) && other.LocalPath.Equals(this.LocalPath))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="CmisSync.Lib.Events.FileDownloadRequest"/> object.
        /// </summary>
        /// <returns>
        /// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.FileDownloadRequest"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.FileDownloadRequest"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("FileDownloadRequest: targetFilePath=\"{0}\"", Path.Combine(this.LocalPath, this.Document.Name));
        }
    }
}
