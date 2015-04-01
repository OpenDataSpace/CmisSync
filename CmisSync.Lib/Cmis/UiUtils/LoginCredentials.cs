
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
    public class LoginCredentials : IComparable {
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
        /// Compares to.
        /// </summary>
        /// <returns>The to.</returns>
        /// <param name="obj">Object.</param>
        public int CompareTo(object obj) {
            if (obj is LoginCredentials) {
                return this.GetPriority(this.FailedException).CompareTo(this.GetPriority((obj as LoginCredentials).FailedException));
            } else {
                throw new ArgumentException("Given credentials are invalid");
            }
        }

        public bool LogIn() {
            return this.LogIn(null);
        }

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

        public override string ToString() {
            return string.Format("[LoginCredentials: Credentials={1}, FailedException={0}]", FailedException, Credentials);
        }

        private int GetPriority(LoginException ex) {
            if (ex == null) {
                return 10;
            } else {
                return (int)ex.Type;
            }
        }
    }
}