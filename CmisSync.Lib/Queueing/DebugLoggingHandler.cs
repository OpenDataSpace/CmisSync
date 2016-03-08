//-----------------------------------------------------------------------
// <copyright file="DebugLoggingHandler.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Events;

    using log4net;

    /// <summary>
    /// Debug logging handler. Does nothing else then calling each toString method of every incoming event.
    /// </summary>
    public class DebugLoggingHandler : SyncEventHandler {
        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DebugLoggingHandler));

        /// <summary>
        /// Handle the specified e.
        /// </summary>
        /// <param name='e'>
        /// Each event will be logged
        /// </param>
        /// <returns>
        /// <c>false</c>
        /// </returns>
        public override bool Handle(ISyncEvent e) {
            if (!(e is IRemoveFromLoggingEvent)) {
                Logger.Debug("Incoming Event: " + e.ToString());
            }

            return false;
        }
    }
}