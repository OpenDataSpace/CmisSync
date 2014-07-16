//-----------------------------------------------------------------------
// <copyright file="IExtendedAttributeReader.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Storage.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Extended attribute reader interface
    /// </summary>
    public interface IExtendedAttributeReader
    {
        /// <summary>
        /// Gets the extended attribute.
        /// </summary>
        /// <returns>
        /// The extended attribute.
        /// </returns>
        /// <param name='path'>
        /// Path.
        /// </param>
        /// <param name='key'>
        /// Key.
        /// </param>
        string GetExtendedAttribute(string path, string key);
        /// <summary>
        /// Sets the extended attribute.
        /// </summary>
        /// <param name='path'>
        /// Path.
        /// </param>
        /// <param name='key'>
        /// Key.
        /// </param>
        /// <param name='value'>
        /// Value.
        /// </param>
        void SetExtendedAttribute(string path, string key, string value);
        /// <summary>
        /// Removes the extended attribute.
        /// </summary>
        /// <param name='path'>
        /// Path.
        /// </param>
        /// <param name='key'>
        /// Key.
        /// </param>
        void RemoveExtendedAttribute(string path, string key);
        /// <summary>
        /// Lists the attribute keys.
        /// </summary>
        /// <returns>
        /// The attribute keys.
        /// </returns>
        /// <param name='path'>
        /// Path.
        /// </param>
        List<string> ListAttributeKeys(string path);

        /// <summary>
        /// Determines whether Extended Attributes are active on the filesystem.
        /// </summary>
        /// <param name="path">Path to be checked</param>
        /// <returns>
        /// <c>true</c> if Extended Attributes are active on the filesystem; otherwise, <c>false</c>.
        /// </returns>
        bool IsFeatureAvailable(string path);
    }

    /// <summary>
    /// Wrong platform exception should be thrown if the executing platfom
    /// is not the target platform of the compilation
    /// </summary>
    [Serializable]
    public class WrongPlatformException : Exception {
        public WrongPlatformException() { }
        public WrongPlatformException(string message) : base(message) { }
        public WrongPlatformException(string message, Exception inner) : base(message, inner) { }
        protected WrongPlatformException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
