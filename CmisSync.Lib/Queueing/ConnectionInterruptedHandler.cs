//-----------------------------------------------------------------------
// <copyright file="ConnectionInterruptedHandler.cs" company="GRAU DATA AG">
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
﻿
namespace CmisSync.Lib.Queueing {
    using System;

    using CmisSync.Lib.Events;

    using DotCMIS.Exceptions;

    /// <summary>
    /// Connection interrupted handler takes <see cref="DotCMIS.Exceptions.CmisConnectionException"/>s from
    /// <see cref="CmisSync.Lib.Queueing.ISyncEventManager"/> and adds them as <see cref="CmisConnectionExceptionEvent"/>s to the given
    /// <see cref="CmisSync.Lib.Queueing.ISyncEventQueue"/>.
    /// </summary>
    public class ConnectionInterruptedHandler {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Queueing.ConnectionInterruptedHandler"/> class.
        /// Registers a function to pass every <see cref="DotCMIS.Exceptions.CmisConnectionException"/> as event to the queue.
        /// </summary>
        /// <param name="manager">Sync event manager.</param>
        /// <param name="queue">Sync event queue.</param>
        public ConnectionInterruptedHandler(ISyncEventManager manager, ISyncEventQueue queue) {
            if (manager == null) {
                throw new ArgumentNullException("manager");
            }

            if (queue == null) {
                throw new ArgumentNullException("queue");
            }

            manager.OnException += (object sender, System.Threading.ThreadExceptionEventArgs e) => {
                var connectionException = e.Exception as CmisConnectionException;
                if (connectionException != null) {
                    queue.AddEvent(new CmisConnectionExceptionEvent(connectionException));
                }
            };
        }
    }
}