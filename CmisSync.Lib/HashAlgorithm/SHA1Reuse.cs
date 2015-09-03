//-----------------------------------------------------------------------
// <copyright file="SHA1Reuse.cs" company="GRAU DATA AG, EMC Corporation">
//   GRAU DATA AG:
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
//   EMC Corporation:
//
//Copyright (c) 2008, EMC Corporation.
//    Redistribution and use in source and binary forms, with or without modification,
//    are permitted provided that the following conditions are met:
//
//        + Redistributions of source code must retain the above copyright notice,
//        this list of conditions and the following disclaimer.
//        + Redistributions in binary form must reproduce the above copyright
//        notice, this list of conditions and the following disclaimer in the
//        documentation and/or other materials provided with the distribution.
//        + The name of EMC Corporation may not be used to endorse or promote
//        products derived from this software without specific prior written
//        permission.
//
//        THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
//        "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
//        TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
//        PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS
//        BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
//        CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
//                               SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
//                               INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
//        CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
//        ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
//        POSSIBILITY OF SUCH DAMAGE.
//
// </copyright>
//-----------------------------------------------------------------------

namespace CmisSync.Lib.HashAlgorithm {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Cloneable SHA1 implementation based on SHA1Managed
    /// </summary>
    public class SHA1Reuse : SHA1Managed, IReusableHashAlgorithm {
        private bool disposed;

        /// <summary>
        /// Clone this instance with its internal states.
        /// </summary>
        /// <returns>A full clone of the actual instance and state.</returns>
        public object Clone() {
            if (this.disposed) {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            return this.DeepCopy(this);
        }

        private object DeepCopy(object source) {
            var type = source.GetType();
            var constructor = type.GetConstructor(System.Type.EmptyTypes);
            if (constructor == null) {
                throw new ArgumentException("Object is not copyable: " + source);
            }

            var clone = (object)constructor.Invoke(new object[0]);

            while (type != null) {
                foreach (var fieldInfo in type.GetFields(BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) {
                    object value = fieldInfo.GetValue(source);
                    if (value is Array) {
                        // Copy array
                        var newValue = Array.CreateInstance(value.GetType().GetElementType(), ((Array)value).Length);
                        Array.Copy((Array)value, newValue, newValue.Length);
                        value = newValue;
                    } else if (value == null || value.GetType().IsPrimitive) {
                        // Ignore
                    } else {
                        value = this.DeepCopy(value);
                    }

                    fieldInfo.SetValue(clone, value);
                }

                type = type.BaseType;
            }

            return clone;
        }

        /// <summary>
        /// Dispose the hash algorithm
        /// </summary>
        /// <param name="disposing">If set to <c>true</c> disposing.</param>
        protected override void Dispose(bool disposing) {
            if (disposing) {
                this.disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}