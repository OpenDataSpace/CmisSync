//-----------------------------------------------------------------------
// <copyright file="ExceptionEvent.cs" company="GRAU DATA AG">
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
    /// Exception event.
    /// </summary>
    public class ExceptionEvent : ISyncEvent {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.ExceptionEvent"/> class.
        /// </summary>
        /// <param name='e'>
        /// The exception, which should be embedded. Must not be null.
        /// </param>
        public ExceptionEvent(Exception e) {
            if (e == null) {
                throw new ArgumentNullException("Given Exception is null");
            }

            this.Exception = e;
        }

        /// <summary>
        /// Gets the exception.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.ExceptionEvent"/> containing the Message of the embedded exception.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.ExceptionEvent"/>.
        /// </returns>
        public override string ToString() {
            return this.Exception.Message;
        }
    }
}