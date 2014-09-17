//-----------------------------------------------------------------------
// <copyright file="RecentChangedEvent.cs" company="GRAU DATA AG">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Recent changed event.
    /// </summary>
    public class RecentChangedEvent : ISyncEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.RecentChangedEvent"/> class.
        /// </summary>
        /// <param name="path">Path of the change.</param>
        public RecentChangedEvent(string path) : this(path, DateTime.Now) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.RecentChangedEvent"/> class.
        /// </summary>
        /// <param name="path">Path of the change.</param>
        /// <param name="changeTime">Change time.</param>
        public RecentChangedEvent(string path, DateTime? changeTime) {
            this.Path = path;
            this.ChangeTime = changeTime != null ? (DateTime)this.ChangeTime : DateTime.Now;
        }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <value>The path.</value>
        public string Path { get; private set; }

        /// <summary>
        /// Gets the change time.
        /// </summary>
        /// <value>The change time.</value>
        public DateTime ChangeTime { get; private set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.RecentChangedEvent"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.RecentChangedEvent"/>.</returns>
        public override string ToString()
        {
            return string.Format("RecentChangedEvent: {0} at {1}", this.Path, this.ChangeTime.ToLongTimeString());
        }
    }
}
