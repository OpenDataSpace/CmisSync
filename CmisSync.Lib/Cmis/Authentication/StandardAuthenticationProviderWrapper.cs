//-----------------------------------------------------------------------
// <copyright file="StandardAuthenticationProviderWrapper.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Cmis {
    using System;
    using System.Net;

    using DotCMIS.Binding;

    /// <summary>
    /// Standard authtentication provider.
    /// </summary>
    public class StandardAuthenticationProviderWrapper : DotCMIS.Binding.StandardAuthenticationProvider, IDisposableAuthProvider {
        /// <summary>
        /// Releases all resource used by the <see cref="StandardAuthenticationProviderWrapper"/> object.
        /// </summary>
        /// <remarks>
        /// Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="StandardAuthenticationProviderWrapper"/>. The <see cref="Dispose"/> method leaves the
        /// <see cref="StandardAuthenticationProviderWrapper"/> in an unusable state. After calling
        /// <see cref="Dispose()"/>, you must release all references to the
        /// <see cref="StandardAuthenticationProviderWrapper"/> so the garbage collector can reclaim the
        /// memory that the <see cref="StandardAuthenticationProviderWrapper"/> was occupying.
        /// </remarks>
        public void Dispose() {
        }

        /// <summary>
        /// Deletes all cookies.
        /// </summary>
        public void DeleteAllCookies() {
            this.Cookies = new CookieContainer();
        }
    }
}