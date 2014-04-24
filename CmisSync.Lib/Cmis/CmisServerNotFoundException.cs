//-----------------------------------------------------------------------
// <copyright file="CmisServerNotFoundException.cs" company="GRAU DATA AG">
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
using System;
using System.Runtime.Serialization;

namespace CmisSync.Lib.Cmis
{
    /// <summary>
    /// Exception launched when the CMIS server can not be found.
    /// </summary>
    [Serializable]
    public class CmisServerNotFoundException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisServerNotFoundException() { }


        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisServerNotFoundException(string message) : base(message) { }


        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisServerNotFoundException(string message, Exception inner) : base(message, inner) { }


        /// <summary>
        /// Constructor.
        /// </summary>
        protected CmisServerNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

}
