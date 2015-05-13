//-----------------------------------------------------------------------
// <copyright file="ConfigurationNeededEvent.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Config;

    /// <summary>
    /// Configuration needed event can be used to inform about a wrong config.
    /// </summary>
    public class ConfigurationNeededEvent : ExceptionEvent {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.ConfigurationNeededEvent"/> class.
        /// </summary>
        /// <param name="e">Exception which requires a change of a configuration.</param>
        public ConfigurationNeededEvent(Exception e) : base(e) {
        }
    }
}