//-----------------------------------------------------------------------
// <copyright file="AuthenticationType.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Config {
    using System;

    /// <summary>
    /// Authentication type.
    /// </summary>
    [Serializable]
    public enum AuthenticationType {
        /// <summary>
        /// The default auth mechanism is HTTP Basic Auth.
        /// </summary>
        BASIC,

        /// <summary>
        /// NTLM auth mechanism.
        /// </summary>
        NTLM,

        /// <summary>
        /// The Kerberos auth mechanism.
        /// </summary>
        KERBEROS,

        /// <summary>
        /// The OAuth mechanism. It is not implemented yet.
        /// </summary>
        OAUTH,

        /// <summary>
        /// The SHIBBOLETH auth mechanism. It is not implemented yet.
        /// </summary>
        SHIBBOLETH,

        /// <summary>
        /// The x501 auth mechanism. It is not implemented yet.
        /// </summary>
        X501,

        /// <summary>
        /// The PGP based auth mechanism. It is not implemented/specified/invented yet.
        /// </summary>
        PGP
    }
}