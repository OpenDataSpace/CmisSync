//-----------------------------------------------------------------------
// <copyright file="CertPolicyWindow.cs" company="GRAU DATA AG">
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

namespace CmisSync {
    using System;
    using System.Threading;

    using Gtk;

    using log4net;

    /// <summary>
    /// SSL Certification policy window.
    /// </summary>
    public class CertPolicyWindow {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CertPolicyWindow));

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.CertPolicyWindow"/> class.
        /// </summary>
        /// <param name="handler">Cert policy handler.</param>
        public CertPolicyWindow(CertPolicyHandler handler) {
            this.Handler = handler;
            this.Handler.ShowWindowEvent += this.ShowCertDialog;
        }

        private CertPolicyHandler Handler { get; set; }

        private void ShowCertDialog() {
            Logger.Debug("Showing Cert Dialog: " + this.Handler.UserMessage);
            CertPolicyHandler.Response ret = CertPolicyHandler.Response.None;
            using (var handle = new AutoResetEvent(false)) {
                Application.Invoke(delegate {
                    try {
                        using (MessageDialog md = new MessageDialog(
                            null,
                            DialogFlags.Modal,
                            MessageType.Warning,
                            ButtonsType.None,
                            this.Handler.UserMessage + "\n\nDo you trust this certificate?") {
                            Title = "Untrusted Certificate" }) {
                            using (var noButton = md.AddButton("No", (int)CertPolicyHandler.Response.CertDeny))
                            using (var justNowButton = md.AddButton("Just now", (int)CertPolicyHandler.Response.CertAcceptSession))
                            using (var alwaysButton = md.AddButton("Always", (int)CertPolicyHandler.Response.CertAcceptAlways)) {
                                ret = (CertPolicyHandler.Response)md.Run();
                                md.Destroy();
                            }
                        }
                    } finally {
                        handle.Set();
                    }
                });
                handle.WaitOne();
            }

            Logger.Debug("Cert Dialog return:" + ret.ToString());
            this.Handler.UserResponse = ret;
        }
    }
}