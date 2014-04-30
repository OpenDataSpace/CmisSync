//-----------------------------------------------------------------------
// <copyright file="ExtendedAttributeException.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Storage
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;

    /// <summary>
    /// Extended attribute exception.
    /// </summary>
    [Serializable]
    public class ExtendedAttributeException : IOException
    {
        public ExtendedAttributeException() : base("ExtendedAttribute manipulation exception") { }
        public ExtendedAttributeException(string msg) : base(msg) { }
        public ExtendedAttributeException(string message, Exception inner) : base (message, inner) { }
        protected ExtendedAttributeException(SerializationInfo info, StreamingContext context) : base (info, context) { }
    }
}
