//-----------------------------------------------------------------------
// <copyright file="AboutController.cs" company="GRAU DATA AG">
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
//   CmisSync, a collaboration and sharing tool.
//   Copyright (C) 2010  Hylke Bons <hylkebons@gmail.com>
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General Public License for more details.
//
//   You should have received a copy of the GNU General Public License
//   along with this program. If not, see <http://www.gnu.org/licenses/>.

namespace CmisSync {
    using System;
    using System.Net;
    using System.Threading;

    using CmisSync.Lib;

    /// <summary>
    /// Controller for the About dialog.
    /// </summary>
    public class AboutController {
        // ===== Actions =====
        /// <summary>
        /// Show About Windows Action
        /// </summary>
        public event Action ShowWindowEvent = delegate { };
        /// <summary>
        /// Hide About Windows Action
        /// </summary>
        public event Action HideWindowEvent = delegate { };

        /// <summary>
        /// URL addresses to display in the About dialog.
        /// </summary>
        public readonly string WebsiteLinkAddress       = "http://www.graudata.com/";
        /// <summary>
        /// URL to the AUTHORS file
        /// </summary>
        public readonly string CreditsLinkAddress       = "https://raw.github.com/OpenDataSpace/CmisSync/master/legal/AUTHORS.txt";

        /// <summary>
        /// Constructor.
        /// </summary>
        public AboutController() {
            Program.Controller.ShowAboutWindowEvent += delegate {
                ShowWindowEvent();
            };
        }


        /// <summary>
        /// Get the CmisSync version.
        /// </summary>
        public string RunningVersion {
            get {
                return Backend.Version;
            }
        }

        public DateTime? CreateTime {
            get {
                return Backend.RetrieveLinkerTimestamp;
            }
        }

        /// <summary>
        /// Closing the dialog.
        /// </summary>
        public void WindowClosed() {
            HideWindowEvent();
        }
    }
}