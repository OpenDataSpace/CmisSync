//-----------------------------------------------------------------------
// <copyright file="FileConflictEvent.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Events {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// File conflict types.
    /// </summary>
    public enum FileConflictType {
        /// <summary>
        /// Remote File Deleted
        /// </summary>
        DELETED_REMOTE_FILE,

        /// <summary>
        /// Remote File Moved
        /// </summary>
        MOVED_REMOTE_FILE,

        /// <summary>
        /// Remote File already existed
        /// </summary>
        ALREADY_EXISTS_REMOTELY,

        /// <summary>
        /// Content was modified
        /// </summary>
        CONTENT_MODIFIED,

        /// <summary>
        /// Remote Path Deleted
        /// </summary>
        DELETED_REMOTE_PATH,

        /// <summary>
        /// Remote added path conflicts with local file
        /// </summary>
        REMOTE_ADDED_PATH_CONFLICTS_LOCAL_FILE
    }

    /// <summary>
    /// File conflict event.
    /// </summary>
    public class FileConflictEvent : ISyncEvent {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.FileConflictEvent"/> class.
        /// </summary>
        /// <param name='type'>
        /// Conflict Type.
        /// </param>
        /// <param name='affectedPath'>
        /// Affected path.
        /// </param>
        /// <param name='createdConflictPath'>
        /// Created conflict path.
        /// </param>
        public FileConflictEvent(FileConflictType type, string affectedPath, string createdConflictPath = null) {
            if (affectedPath == null) {
                throw new ArgumentNullException("affectedPath", "Argument null in FileConflictEvent Constructor");
            }

            this.Type = type;
            this.AffectedPath = affectedPath;
            this.CreatedConflictPath = createdConflictPath;
        }

        /// <summary>
        /// Gets the type of the conflict.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public FileConflictType Type { get; private set; }

        /// <summary>
        /// Gets the affected path.
        /// </summary>
        /// <value>
        /// The affected path.
        /// </value>
        public string AffectedPath { get; private set; }

        /// <summary>
        /// Gets the created conflict path.
        /// </summary>
        /// <value>
        /// The created conflict path.
        /// </value>
        public string CreatedConflictPath { get; private set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.FileConflictEvent"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.FileConflictEvent"/>.
        /// </returns>
        public override string ToString() {
            if (this.CreatedConflictPath == null) {
                return string.Format("FileConflictEvent: \"{0}\" on path \"{1}\"", this.Type, this.AffectedPath);
            } else {
                return string.Format("FileConflictEvent: \"{0}\" on path \"{1}\" solved by creating path \"{2}\"", this.Type, this.AffectedPath, this.CreatedConflictPath);
            }
        }
    }
}