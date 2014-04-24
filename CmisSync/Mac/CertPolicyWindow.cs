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
using System.Threading;
using MonoMac.Foundation;
using MonoMac.AppKit;
using log4net;

namespace CmisSync
{
    class CertPolicyWindow : NSObject
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CertPolicyWindow));

        private CertPolicyHandler Handler { get; set; }

        public CertPolicyWindow(CertPolicyHandler handler)
        {
            Handler = handler;
            Handler.ShowWindowEvent += ShowCertDialog;
        }

        private void ShowCertDialog()
        {
            Logger.Debug("Showing Cert Dialog: " + Handler.UserMessage);
            CertPolicyHandler.Response ret = CertPolicyHandler.Response.None;
            using (var signal = new AutoResetEvent(false))
            {
                InvokeOnMainThread(delegate
                {
                    try
                    {
                        NSAlert alert = NSAlert.WithMessage("Untrusted Certificate", "No", "Always", "Just now", Handler.UserMessage + "\n\nDo you trust this certificate?");
                        switch (alert.RunModal())
                        {
                            case 1:
                                ret = CertPolicyHandler.Response.CertDeny;
                                break;
                            case 0:
                                ret = CertPolicyHandler.Response.CertAcceptAlways;
                                break;
                            case -1:
                                ret = CertPolicyHandler.Response.CertAcceptSession;
                                break;
                            default:
                                ret = CertPolicyHandler.Response.None;
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        ret = CertPolicyHandler.Response.None;
                    }
                    finally
                    {
                        signal.Set();
                    }
                });
                signal.WaitOne();
            }
            Logger.Debug("Cert Dialog return:" + ret.ToString());
            Handler.UserResponse = ret;
        }
    }
}

