//-----------------------------------------------------------------------
// <copyright file="EntryNotFoundException.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Storage.Database
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Db requeted Entry not found exception.
    /// </summary>
    [Serializable]
    public class EntryNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntryNotFoundException"/> class.
        /// </summary>
        public EntryNotFoundException() : base() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntryNotFoundException"/> class.
        /// </summary>
        /// <param name='message'>
        /// Message of the exception.
        /// </param>
        public EntryNotFoundException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntryNotFoundException"/> class.
        /// </summary>
        /// <param name='message'>
        /// Message of the exception.
        /// </param>
        /// <param name='inner'>
        /// Inner Exception.
        /// </param>
        public EntryNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntryNotFoundException"/> class.
        /// </summary>
        /// <param name='info'>
        /// Serialization Info.
        /// </param>
        /// <param name='context'>
        /// Serialization Context.
        /// </param>
        protected EntryNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
