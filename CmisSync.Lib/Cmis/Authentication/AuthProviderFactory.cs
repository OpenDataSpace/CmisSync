//-----------------------------------------------------------------------
// <copyright file="AuthProviderFactory.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Cmis
{
    using System;
    using System.Net;

    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DBreeze;

    /// <summary>
    /// Auth provider factory.
    /// </summary>
    public static class AuthProviderFactory
    {
        /// <summary>
        /// Creates the auth provider fitting to the given type and url
        /// </summary>
        /// <returns>
        /// The auth provider.
        /// </returns>
        /// <param name='type'>
        /// Authentication type.
        /// </param>
        /// <param name='url'>
        /// service url.
        /// </param>
        /// <param name='db'>
        /// storage engine
        /// </param>
        [CLSCompliant(false)]
        public static IDisposableAuthProvider CreateAuthProvider(Config.AuthenticationType type, Uri url, DBreezeEngine db)
        {
            ICookieStorage storage = new PersistentCookieStorage(db);

            switch(type)
            {
            case Config.AuthenticationType.BASIC:
                return new PersistentStandardAuthenticationProvider(storage, url);
            case Config.AuthenticationType.KERBEROS:
                goto case Config.AuthenticationType.NTLM;
            case Config.AuthenticationType.NTLM:
                return new PersistentNtlmAuthenticationProvider(storage, url);
            default:
                return new StandardAuthenticationProviderWrapper();
            }
        }
    }
}
