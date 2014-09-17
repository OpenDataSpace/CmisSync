//-----------------------------------------------------------------------
// <copyright file="FSEvent.cs" company="GRAU DATA AG">
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

    /// <summary>
    /// FS event.
    /// </summary>
    /// <exception cref='ArgumentNullException'>
    /// Is thrown when an argument passed to a method is invalid because it is <see langword="null" /> .
    /// </exception>
    public class FSEvent : IFSEvent
    {
        /// <summary>
        /// Gets a value indicating whether this instance is pointing to a directory.
        /// </summary>
        /// <value><c>true</c> if this is directory; otherwise, <c>false</c>.</value>
        public bool IsDirectory { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.FSEvent"/> class.
        /// </summary>
        /// <param name='type'>
        /// The Type.
        /// </param>
        /// <param name='path'>
        /// The Path.
        /// </param>
        /// <param name="isDirectory">
        /// Signalizes if the target of the path is a directory or file.
        /// </param>
        /// <exception cref='ArgumentNullException'>
        /// Is thrown when an argument passed to a method is invalid because it is <see langword="null" /> .
        /// </exception>
        public FSEvent(WatcherChangeTypes type, string path, bool isDirectory)
        {
            if (path == null) {
                throw new ArgumentNullException("Argument null in FSEvent Constructor", "path");
            }

            this.Type = type;
            FileSystemInfo fileSystemInfo = isDirectory ? (FileSystemInfo)new DirectoryInfo(path) : (FileSystemInfo)new FileInfo(path);
            this.LocalPath = fileSystemInfo.FullName;
            this.IsDirectory = isDirectory;
            this.Name = fileSystemInfo.Name;
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public WatcherChangeTypes Type { get; private set; }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string LocalPath { get; private set; }

        /// <summary>
        /// Gets the name of the file or directory.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.FSEvent"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.FSEvent"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("FSEvent with type \"{0}\" on path \"{1}\" and the name \"{2}\"", this.Type, this.LocalPath, this.Name);
        }
    }
}
