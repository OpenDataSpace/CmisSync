//-----------------------------------------------------------------------
// <copyright file="IDisposableSyncEventQueue.cs" company="GRAU DATA AG">
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
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    using log4net;

    /// <summary>
    /// Interface of a disposable sync event queue.
    /// </summary>
    public interface IDisposableSyncEventQueue : ISyncEventQueue, IDisposable {
        /// <summary>
        /// Stops the listeners.
        /// </summary>
        void StopListener();

        /// <summary>
        /// Waits for queue to be stopped.
        /// </summary>
        /// <returns><c>true</c>, if for stopped was waited, <c>false</c> otherwise.</returns>
        /// <param name="timeout">Timeout of waiting.</param>
        bool WaitForStopped(int timeout);
    }
}