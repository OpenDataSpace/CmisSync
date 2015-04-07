//-----------------------------------------------------------------------
// <copyright file="SettingController.cs" company="GRAU DATA AG">
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


namespace CmisSync {

    /// <summary>
    /// Controller for the Setting dialog.
    /// </summary>
    public class SettingController {

        //===== Actions =====
        /// <summary>
        /// Show Setting Windows Action
        /// </summary>
        public event Action ShowWindowEvent = delegate { };
        /// <summary>
        /// Hide Setting Windows Action
        /// </summary>
        public event Action HideWindowEvent = delegate { };

        public event Action<bool> CheckProxyNoneEvent = delegate { };
        public event Action<bool> CheckProxySystemEvent = delegate { };
        public event Action<bool> CheckProxyCutomEvent = delegate { };
        public event Action<string> UpdateServerHelpEvent = delegate { };
        public event Action<bool> EnableLoginEvent = delegate { };
        public event Action<bool> CheckLoginEvent = delegate { };
        public event Action<bool> UpdateSaveEvent = delegate { };

        /// <summary>
        /// Constructor.
        /// </summary>
        public SettingController() {
            Program.Controller.ShowSettingWindowEvent += delegate {
                ShowWindowEvent();
            };
        }

        public void ShowWindow() {
            ShowWindowEvent();
        }

        public void HideWindow() {
            HideWindowEvent();
        }

        public void CheckProxyNone() {
            CheckProxyNoneEvent(true);
            CheckProxySystemEvent(false);
            CheckProxyCutomEvent(false);
            EnableLoginEvent(false);
        }

        public void CheckProxySystem() {
            CheckProxyNoneEvent(false);
            CheckProxySystemEvent(true);
            CheckProxyCutomEvent(false);
            EnableLoginEvent(true);
        }

        public void CheckProxyCustom() {
            CheckProxyNoneEvent(false);
            CheckProxySystemEvent(false);
            CheckProxyCutomEvent(true);
            EnableLoginEvent(true);
        }

        public void CheckLogin(bool check) {
            CheckLoginEvent(check);
        }

        public string GetServer(string server) {
            try {
                Uri uri = new Uri(server);
                return server;
            } catch (Exception) {
                if (!server.StartsWith("http://")) {
                    server = "http://" + server;
                    try {
                        Uri uri = new Uri(server);
                        return server;
                    } catch(Exception) {
                    }
                }
            }

            return null;
        }

        public void ValidateServer(string server) {
            if (GetServer(server) == null) {
                UpdateServerHelpEvent(Properties_Resources.InvalidURL);
                UpdateSaveEvent(false);
            } else {
                UpdateServerHelpEvent(string.Empty);
                UpdateSaveEvent(true);
            }
        }
    }
}