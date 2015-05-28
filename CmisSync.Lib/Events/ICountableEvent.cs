//-----------------------------------------------------------------------
// <copyright file="ICountableEvent.cs" company="GRAU DATA AG">
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

    /// <summary>
    /// Instances of this event are able to be counted by categories.
    /// Every event, which should be able to be tracked inside the SyncEventQueue must return a category.
    /// </summary>
    public interface ICountableEvent : ISyncEvent {
        /// <summary>
        /// Gets the category of the event. This can be used to differ between multiple event types.
        /// The returned value should never ever change its value after requesting it the first time.
        /// </summary>
        /// <value>The event category.</value>
        EventCategory Category { get; }
    }
}