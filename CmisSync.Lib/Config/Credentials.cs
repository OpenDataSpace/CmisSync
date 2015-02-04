//-----------------------------------------------------------------------
// <copyright file="Credentials.cs" company="GRAU DATA AG">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using CmisSync.Lib.Cmis;

    /// <summary>
    /// Typical user credantials used for generic logins
    /// </summary>
    [Serializable]
    public class UserCredentials {
        /// <summary>
        /// Gets or sets the user name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password
        /// </summary>
        public Password Password { get; set; }
    }

    /// <summary>
    /// Server Login for a specific Uri
    /// </summary>
    [Serializable]
    public class ServerCredentials : UserCredentials {
        /// <summary>
        /// Gets or sets the server Address and Path
        /// </summary>
        public Uri Address { get; set; }

        public static readonly string BindingAtomPub = DotCMIS.BindingType.AtomPub;
        public static readonly string BindingBrowser = DotCMIS.BindingType.Browser;

        private string binding = BindingAtomPub;

        /// <summary>
        /// Gets or sets the CMIS binding
        /// </summary>
        public string Binding {
            get {
                return binding;
            }
            set {
                binding = value;
            }
        }
    }

    /// <summary>
    /// Credentials needed to create a Session for a specific CMIS repository
    /// </summary>
    [Serializable]
    public class CmisRepoCredentials : ServerCredentials {
        /// <summary>
        /// Gets or sets the repository identifier.
        /// </summary>
        /// <value>The repository identifier.</value>
        public string RepoId { get; set; }
    }

    /// <summary>
    /// Password class stores the given password obfuscated
    /// </summary>
    [Serializable]
    public class Password {
        private string password = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Password"/> class with the given password.
        /// </summary>
        /// <param name="password">as plain text</param>
        public Password(string password) {
            this.password = Crypto.Obfuscate(password);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Password"/> class without setting a password.
        /// </summary>
        public Password() {
        }

        /// <summary>
        /// Gets or sets the internal saved and obfuscated password
        /// </summary>
        public string ObfuscatedPassword {
            get { return this.password; }
            set { this.password = value; }
        }

        /// <summary>
        /// Implizit contructor for passing a plain text string as password
        /// </summary>
        /// <param name="value">plain text password</param>
        /// <returns></returns>
        public static implicit operator Password(string value) {
            return new Password(value);
        }

        /// <summary>
        /// Returns the password as plain text
        /// </summary>
        /// <returns>plain text password</returns>
        public override string ToString() {
            return this.password == null ? null : Crypto.Deobfuscate(this.password);
        }
    }
}