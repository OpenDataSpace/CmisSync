//-----------------------------------------------------------------------
// <copyright file="ToxiproxyAuthenticationProviderWrapper.cs" company="GRAU DATA AG">
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
namespace TestLibrary.TestUtils.ToxiproxyUtils {
    using System;

    using DotCMIS.Binding;

    public class ToxiproxyAuthenticationProviderWrapper : IAuthenticationProvider {
        private readonly IAuthenticationProvider original;

        public event Action<object> OnAuthenticate;

        public event Action<object> OnResponse;

        public ToxiproxyAuthenticationProviderWrapper(IAuthenticationProvider orig) {
            if (orig == null) {
                throw new ArgumentNullException("orig");
            }

            this.original = orig;
        }

        public void Authenticate(object connection) {
            this.original.Authenticate(connection);
            var handler = OnAuthenticate;
            if (handler != null) {
                handler(connection);
            }
        }

        public void HandleResponse(object connection) {
            this.original.HandleResponse(connection);
            var handler = OnResponse;
            if (handler != null) {
                handler(connection);
            }
        }

        public IBindingSession Session {
            get {
                return this.original.Session;
            }

            set {
                this.original.Session = value;
            }
        }
    }
}