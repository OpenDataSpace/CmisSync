//-----------------------------------------------------------------------
// <copyright file="RemoteEvent.cs" company="GRAU DATA AG">
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

    using DotCMIS.Client;
    using DotCMIS.Enums;

    /// <summary>
    /// Remote event.
    /// </summary>
    public class RemoteEvent : ISyncEvent
    {
        private IChangeEvent change;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.RemoteEvent"/> class.
        /// </summary>
        /// <param name="change">Change event.</param>
        public RemoteEvent(IChangeEvent change)
        {
            if(change == null) {
                throw new ArgumentNullException("The given change event must not be null");
            }

            this.change = change;
        }

        /// <summary>
        /// Gets the change event.
        /// </summary>
        /// <value>The change.</value>
        public IChangeEvent Change {
            get { return this.change; }
        }

        /// <summary>
        /// Gets the object identifier.
        /// </summary>
        /// <value>The object identifier.</value>
        public string ObjectId {
            get { return this.change.ObjectId; }
        }

        /// <summary>
        /// Gets the change type.
        /// </summary>
        /// <value>The type.</value>
        public DotCMIS.Enums.ChangeType? Type {
            get { return this.change.ChangeType; }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.RemoteEvent"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.RemoteEvent"/>.</returns>
        public override string ToString()
        {
            return string.Format("[RemoteEvent: ChangeType={0} ObjectId={1}]", this.Type, this.ObjectId);
        }
    }
}