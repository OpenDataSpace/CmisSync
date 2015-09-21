//-----------------------------------------------------------------------
// <copyright file="ITransmissionAggregator.cs" company="GRAU DATA AG">
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
namespace CmisSync.Lib.FileTransmission {
    using System;

    /// <summary>
    /// Transmission aggregator interface.
    /// </summary>
    public interface ITransmissionAggregator {
        /// <summary>
        /// Adds a transmission to Aggregator.
        /// </summary>
        /// <param name="transmission">Transmission which should be aggregated with others.</param>
        void Add(Transmission transmission);
    }
}