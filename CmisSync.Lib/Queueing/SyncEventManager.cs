//-----------------------------------------------------------------------
// <copyright file="SyncEventManager.cs" company="GRAU DATA AG">
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
namespace CmisSync.Lib.Queueing {
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Events;

    using log4net;

    /// <summary>
    /// Sync event manager which has a list of all Handlers and forwards events to them.
    /// </summary>
    public class SyncEventManager : ISyncEventManager {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SyncEventManager));
        private List<SyncEventHandler> handler = new List<SyncEventHandler>();

        /// <summary>
        /// Adds the event handler to the manager.
        /// </summary>
        /// <param name='handler'>
        /// Handler to add.
        /// </param>
        public void AddEventHandler(SyncEventHandler handler) {
            // The zero-based index of item in the sorted List<T>,
            // if item is found; otherwise, a negative number that
            // is the bitwise complement of the index of the next
            // element that is larger than item or.
            int pos = this.handler.BinarySearch(handler);
            if (pos < 0) {
                pos = ~pos;
            }

            this.handler.Insert(pos, handler);
        }

        /// <summary>
        /// Handle the specified event.
        /// </summary>
        /// <param name='e'>
        /// Event to handle.
        /// </param>
        public void Handle(ISyncEvent e) {
            for (int i = this.handler.Count - 1; i >= 0; i--) {
                var h = this.handler[i];
                if (h.Handle(e)) {
                    if (!(e is IRemoveFromLoggingEvent)) {
                        Logger.Debug(string.Format("Event {0} was handled by {1}", e.ToString(), h.GetType()));
                    }

                    return;
                }
            }
        }

        /// <summary>
        /// Removes the event handler.
        /// </summary>
        /// <param name='handler'>
        /// Handler to remove.
        /// </param>
        public void RemoveEventHandler(SyncEventHandler handler) {
            this.handler.Remove(handler);
        }
    }
}