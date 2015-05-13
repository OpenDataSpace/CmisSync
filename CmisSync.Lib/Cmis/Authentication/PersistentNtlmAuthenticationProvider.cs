//-----------------------------------------------------------------------
// <copyright file="PersistentNtlmAuthenticationProvider.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Storage.Database;

    using DotCMIS.Binding;

    using log4net;

    // TODO Refactore this class because it is a simple copy of PersistentStandardAuthenticationProvider
    // => Extract methods and call them instead of the duplicated code

    /// <summary>
    /// Persistent ntlm authentication provider.
    /// </summary>
    public class PersistentNtlmAuthenticationProvider : NtlmAuthenticationProvider, IDisposableAuthProvider {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PersistentStandardAuthenticationProvider));

        private ICookieStorage storage;
        private bool disposed = false;
        private Uri url;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Cmis.PersistentNtlmAuthenticationProvider"/> class.
        /// </summary>
        /// <param name='storage'>
        /// Storage to save the cookies to.
        /// </param>
        /// <param name='url'>
        /// URL of the cookies.
        /// </param>
        public PersistentNtlmAuthenticationProvider(ICookieStorage storage, Uri url) {
            if (storage == null) {
                throw new ArgumentNullException("Given db is null");
            }

            if (url == null) {
                throw new ArgumentNullException("Given URL is null");
            }

            this.storage = storage;
            this.url = url;

            if (storage.Cookies != null) {
                foreach (Cookie c in storage.Cookies) {
                    this.Cookies.Add(c);
                }
            }
        }

        /// <summary>
        /// Handles the response, if it is a <see cref="System.Net.HttpWebResponse"/> instance.
        /// Takes all cookies of the response and saves them at the local <see cref="System.Net.CookieContainer"/>.
        /// </summary>
        /// <param name='connection'>
        /// <see cref="System.Net.HttpWebResponse"/> should be passed.
        /// </param>
        public override void HandleResponse(object connection) {
            HttpWebResponse response = connection as HttpWebResponse;
            if (response != null) {
                // AtomPub and browser binding authentictaion
                this.Cookies.Add(response.Cookies);
            }
        }

        /// <summary>
        /// Releases all resource used by the <see cref="CmisSync.Lib.Cmis.PersistentNtlmAuthenticationProvider"/> object.
        /// </summary>
        /// <remarks>
        /// Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="CmisSync.Lib.Cmis.PersistentNtlmAuthenticationProvider"/>. The <see cref="Dispose"/> method
        /// leaves the <see cref="CmisSync.Lib.Cmis.PersistentNtlmAuthenticationProvider"/> in an unusable state. After
        /// calling <see cref="Dispose"/>, you must release all references to the
        /// <see cref="CmisSync.Lib.Cmis.PersistentNtlmAuthenticationProvider"/> so the garbage collector can reclaim
        /// the memory that the <see cref="CmisSync.Lib.Cmis.PersistentNtlmAuthenticationProvider"/> was occupying.
        /// </remarks>
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Deletes all cookies.
        /// </summary>
        public void DeleteAllCookies() {
            this.Cookies = new CookieContainer();
        }

        /// <summary>
        /// Dispose the specified disposing.
        /// </summary>
        /// <param name='disposing'>
        /// Dispose managed resources if <c>true</c>
        /// </param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    // Dispose managed resources.
                    try {
                        this.storage.Cookies = this.Cookies.GetCookies(this.url);
                    } catch(Exception e) {
                        Logger.Debug(string.Format("Failed to save session cookies of \"{0}\" in db", this.url.AbsolutePath), e);
                    }
                }

                this.disposed = true;
            }
        }
    }
}