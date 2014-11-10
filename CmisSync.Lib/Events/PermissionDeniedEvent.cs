//-----------------------------------------------------------------------
// <copyright file="PermissionDeniedEvent.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Events
{
    using System;

    using DotCMIS.Exceptions;

    /// <summary>
    /// Permission denied event.
    /// </summary>
    public class PermissionDeniedEvent : ExceptionEvent
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.PermissionDeniedEvent"/> class.
        /// </summary>
        /// <param name="e">thrown permission denied exception</param>
        public PermissionDeniedEvent(DotCMIS.Exceptions.CmisPermissionDeniedException e) : base(e)
        {
            if (e.ErrorContent != null) {
                try {
                    long unixMiliSeconds = Convert.ToInt64(e.ErrorContent);
                    this.IsBlockedUntil = PermissionDeniedEvent.UnixEpoch.AddMilliseconds(unixMiliSeconds);
                } catch(FormatException) {
                } catch(OverflowException) {
                }
            }
        }

        /// <summary>
        /// Gets the DateTime until the login is blocked.
        /// </summary>
        /// <value>The blocked until.</value>
        public DateTime? IsBlockedUntil { get; private set; }
    }
}