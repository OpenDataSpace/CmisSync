//-----------------------------------------------------------------------
// <copyright file="UI.cs" company="GRAU DATA AG">
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

    using Gtk;

    [CLSCompliant(false)]
    public class UI : IDisposable {
        private bool disposed = false;
        public StatusIcon StatusIcon;
        public Setup Setup;
        public About About;
        public Setting Setting;
        public TransmissionWindow Transmissions;

        public static string AssetsPath =
            (null != Environment.GetEnvironmentVariable("CMISSYNC_ASSETS_DIR"))
            ? Environment.GetEnvironmentVariable("CMISSYNC_ASSETS_DIR") : Defines.ASSETS_DIR;

        public UI() {
            Application.Init();

            this.Setup      = new Setup();
            this.About      = new About();
            this.StatusIcon = new StatusIcon();
            this.Setting    = new Setting();
            this.Transmissions = new TransmissionWindow();
            Program.Controller.UIHasLoaded();
        }

        // Runs the application
        public void Run() {
            Application.Run();
        }

        public void Dispose() {
            if (this.disposed) {
                return;
            }

            if (this.Setup != null) {
                this.Setup.Dispose();
                this.Setup = null;
            }

            if (this.About != null) {
                this.About.Dispose();
                this.About = null;
            }

            if (this.StatusIcon != null) {
                this.StatusIcon.Dispose();
                this.StatusIcon = null;
            }

            if (this.Setting != null) {
                this.Setting.Dispose();
                this.Setting = null;
            }

            this.disposed = true;
        }
    }
}