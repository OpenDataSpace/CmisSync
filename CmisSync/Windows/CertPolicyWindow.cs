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
using System;

using System.Windows;

using log4net;

namespace CmisSync
{
    class CertPolicyWindow
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CertPolicyWindow));

        private CertPolicyHandler Handler { get; set; }

        public CertPolicyWindow (CertPolicyHandler handler)
        {
            Handler = handler;
            Handler.ShowWindowEvent += ShowCertDialog;
        }

        private void ShowCertDialog() {
            Logger.Debug("Showing Cert Dialog: " + Handler.UserMessage);
            CertPolicyHandler.Response ret = CertPolicyHandler.Response.None;
            var r = MessageBox.Show(Handler.UserMessage +
                "\n\n"+ Properties_Resources.DoYouTrustTheCertificate,
                Properties_Resources.UntrustedCertificate, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            switch (r) {
                case MessageBoxResult.Yes:
                    ret = CertPolicyHandler.Response.CertAcceptAlways;
                    break;
                case MessageBoxResult.No:
                    ret = CertPolicyHandler.Response.CertDeny;
                    break;
                case MessageBoxResult.Cancel:
                    ret = CertPolicyHandler.Response.CertAcceptSession;
                    break;
            }
            Logger.Debug("Cert Dialog return:" + ret.ToString());
            Handler.UserResponse = ret;
        }

    }
}

