//-----------------------------------------------------------------------
// <copyright file="SuccessfulLoginEvent.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Events {
    using System;

    using DotCMIS.Client;

    /// <summary>
    /// Successful login on a server should add this event to the event queue.
    /// </summary>
    public class SuccessfulLoginEvent : ISyncEvent {
        private Uri url;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.SuccessfulLoginEvent"/> class.
        /// </summary>
        /// <param name="url">URL of the successful connection</param>
        /// <param name="session">Session instance.</param>
        /// <param name="rootFolder">Remote sync root folder of the configured sync repository.</param>
        /// <param name="privateWorkingCopySupported">Value which gives informations if the given session supports private working copies.</param>
        public SuccessfulLoginEvent(
            Uri url,
            ISession session,
            IFolder rootFolder,
            bool privateWorkingCopySupported)
        {
            if (url == null) {
                throw new ArgumentNullException("url");
            }

            if (session == null) {
                throw new ArgumentNullException("session");
            }

            if (rootFolder == null) {
                throw new ArgumentNullException("rootFolder");
            }

            this.url = url;
            this.Session = session;
            this.RootFolder = rootFolder;
            this.PrivateWorkingCopySupported = privateWorkingCopySupported;
        }

        /// <summary>
        /// Gets the session.
        /// </summary>
        /// <value>The session.</value>
        public ISession Session { get; private set; }

        /// <summary>
        /// Gets the root folder for the synchronization process.
        /// </summary>
        /// <value>The root folder.</value>
        public IFolder RootFolder { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the session supports private working copy.
        /// </summary>
        /// <value><c>true</c> if private working copy supported; otherwise, <c>false</c>.</value>
        public bool PrivateWorkingCopySupported { get; private set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.SuccessfulLoginEvent"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.SuccessfulLoginEvent"/>.</returns>
        public override string ToString() {
            return string.Format("[SuccessfulLoginEvent {0}]", this.url.ToString());
        }
    }
}