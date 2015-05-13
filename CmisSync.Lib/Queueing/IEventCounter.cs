//-----------------------------------------------------------------------
// <copyright file="IEventCounter.cs" company="GRAU DATA AG">
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

    /// <summary>
    /// event counter interface.
    /// </summary>
    public interface IEventCounter : IDisposable {
        /// <summary>
        /// Increase the counter if event fits.
        /// </summary>
        /// <param name="e">Countable event.</param>
        void Increase(ICountableEvent e);

        /// <summary>
        /// Decrease the counter if event fits.
        /// </summary>
        /// <param name="e">Countable event.</param>
        void Decrease(ICountableEvent e);
    }
}