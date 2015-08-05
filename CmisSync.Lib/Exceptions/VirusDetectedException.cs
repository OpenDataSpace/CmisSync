//-----------------------------------------------------------------------
// <copyright file="VirusDetectedException.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Exceptions {
    using System;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS.Exceptions;

    /// <summary>
    /// Virus detected exception.
    /// </summary>
    [Serializable]
    public class VirusDetectedException : AbstractInteractionNeededException {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Exceptions.VirusDetectedException"/> class.
        /// </summary>
        /// <param name="cmisException">Cmis constraint exception which is thrown because a virus is detected.</param>
        public VirusDetectedException(CmisConstraintException cmisException) : base("Virus Detected", cmisException) {
            if (cmisException == null) {
                throw new ArgumentNullException("cmisException");
            }

            if (!cmisException.IsVirusDetectionException()) {
                throw new ArgumentException("Given exception is not a virus detected exception");
            }

            this.Level = ExceptionLevel.Warning;
        }
    }
}