//-----------------------------------------------------------------------
// <copyright file="ITransmissionFactory.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.FileTransmission {
    using System;

    /// <summary>
    /// Interface for a transmission manager. It is the main factory for new Transmission objects and the management interface for all running transmissions.
    /// </summary>
    public interface ITransmissionFactory {
        /// <summary>
        /// Creates a new the transmission object and adds it to the manager. The manager decides when to and how the transmission gets removed from it.
        /// </summary>
        /// <returns>The transmission.</returns>
        /// <param name="type">Transmission type.</param>
        /// <param name="path">Full path.</param>
        /// <param name="cachePath">Cache path.</param>
        Transmission CreateTransmission(TransmissionType type, string path, string cachePath = null);
    }
}