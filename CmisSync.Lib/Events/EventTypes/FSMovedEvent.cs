//-----------------------------------------------------------------------
// <copyright file="FSMovedEvent.cs" company="GRAU DATA AG">
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
    /// FS moved event.
    /// </summary>
    public class FSMovedEvent : FSEvent, IFSMovedEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.FSMovedEvent"/> class.
        /// </summary>
        /// <param name='oldPath'>
        /// Old path.
        /// </param>
        /// <param name='newPath'>
        /// New path.
        /// </param>
        public FSMovedEvent(string oldPath, string newPath, bool isDirectory) : base(WatcherChangeTypes.Renamed, newPath, isDirectory)
        {
            this.OldPath = oldPath;
        }

        /// <summary>
        /// Gets the old path.
        /// </summary>
        /// <value>
        /// The old path.
        /// </value>
        public string OldPath { get; private set; }
    }
}
