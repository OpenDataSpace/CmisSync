//-----------------------------------------------------------------------
// <copyright file="LoginCredentials.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Cmis.UiUtils {
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Config;

    using DotCMIS.Client;
    using DotCMIS.Client.Impl;
    using DotCMIS.Exceptions;

    /// <summary>
    /// Login credentials helper class for creating new connections and handle result if it fails.
    /// </summary>
    public class LoginCredentials {
        /// <summary>
        /// Gets or sets the failed exception.
        /// </summary>
        /// <value>The failed exception.</value>
        public LoginException FailedException { get; set; }

        /// <summary>
        /// Gets or sets the credentials.
        /// </summary>
        /// <value>The credentials.</value>
        public ServerCredentials Credentials { get; set; }

        /// <summary>
        /// Gets the repositories after Login.
        /// </summary>
        /// <value>The repositories.</value>
        public IList<LogonRepositoryInfo> Repositories { get; private set; }

        /// <summary>
        /// Gets the priority based on the LoginException.
        /// </summary>
        /// <value>The priority.</value>
        public int Priority {
            get {
                if (this.FailedException == null) {
                    return 10;
                } else {
                    return (int)this.FailedException.Type;
                }
            }
        }

        /// <summary>
        /// Tries to log in on with the given credentials
        /// </summary>
        /// <returns><c>true</c>, if login was successful, <c>false</c> otherwise.</returns>
        public bool LogIn() {
            return this.LogIn(null);
        }

        /// <summary>
        /// Tries to log in on with the given credentials
        /// </summary>
        /// <returns><c>true</c>, if login was successful, <c>false</c> otherwise.</returns>
        /// <param name="sessionFactory">Use the given session factory for login. If null is passed, the default DotCMIS SessionFactory is used.</param>
        public bool LogIn(ISessionFactory sessionFactory) {
            // Create session factory if non is given
            var factory = sessionFactory ?? SessionFactory.NewInstance();
            try {
                this.Repositories = this.Credentials.GetRepositories(factory);
                return true;
            } catch (Exception e) {
                this.FailedException = new LoginException(e);
                return false;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Cmis.UiUtils.LoginCredentials"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Cmis.UiUtils.LoginCredentials"/>.</returns>
        public override string ToString() {
            return string.Format("[LoginCredentials: Credentials={1}, FailedException={0}]", this.FailedException, this.Credentials);
        }
    }
}