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
using CmisSync.Lib.Cmis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CmisSync.Lib.Credentials
{
    /// <summary>
    /// Typical user credantials used for generic logins
    /// </summary>
    [Serializable]
    public class UserCredentials
    {
        /// <summary>
        /// User name
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// Password
        /// </summary>
        public Password Password { get; set; }
    }

    /// <summary>
    /// Server Login for a specific Uri
    /// </summary>
    [Serializable]
    public class ServerCredentials : UserCredentials
    {
        /// <summary>
        /// Server Address and Path
        /// </summary>
        public Uri Address { get; set; }
    }

    /// <summary>
    /// Credentials needed to create a Session for a specific CMIS repository
    /// </summary>
    [Serializable]
    public class CmisRepoCredentials : ServerCredentials
    {
        /// <summary>
        /// Repository ID
        /// </summary>
        public string RepoId { get; set; }
    }

    /// <summary>
    /// Password class stores the given password obfuscated
    /// </summary>
    [Serializable]
    public class Password
    {
        private string password = null;
        /// <summary>
        /// Constructor initializing the instance with the given password
        /// </summary>
        /// <param name="password">as plain text</param>
        public Password(string password)
        {
            this.password = Crypto.Obfuscate(password);
        }

        /// <summary>
        /// Default constructor without setting the stored password
        /// </summary>
        public Password() { }

        /// <summary>
        /// Implizit contructor for passing a plain text string as password
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator Password(string value)
        {
            return new Password(value);
        }

        /// <summary>
        /// Returns the password as plain text
        /// </summary>
        /// <returns>plain text password</returns>
        override
        public string ToString()
        {
            if (password == null)
                return null;
            return Crypto.Deobfuscate(password);
        }

        /// <summary>
        /// Gets and sets the internal saved and obfuscated password
        /// </summary>
        public string ObfuscatedPassword { get { return password; } set { password = value; } }
    }
}
