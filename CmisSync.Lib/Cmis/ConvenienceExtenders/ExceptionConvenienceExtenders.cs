//-----------------------------------------------------------------------
// <copyright file="ExceptionConvenienceExtenders.cs" company="GRAU DATA AG">
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
namespace CmisSync.Lib.Cmis.ConvenienceExtenders {
    using System;

    using DotCMIS.Exceptions;

    /// <summary>
    /// Exception convenience extenders.
    /// </summary>
    public static class ExceptionConvenienceExtenders {
        /// <summary>
        /// Determines if a virus detection exception is the reason for the CmisContraintException.
        /// </summary>
        /// <returns><c>true</c> if the given exception seems to be a virus dectection exception; otherwise, <c>false</c>.</returns>
        /// <param name="ex">Cmis constraint Exception.</param>
        public static bool IsVirusDetectionException(this CmisConstraintException ex) {
            if (ex.ErrorContent.Contains("Infected file")) {
                return true;
            } else {
                return false;
            }
        }
    }
}