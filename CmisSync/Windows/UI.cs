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
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Threading;

    using log4net;

    /// <summary>
    /// User interface of CmisSync.
    /// </summary>
    public class UI {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(UI));

        /// <summary>
        /// Dialog shown at first run to explain how CmisSync works.
        /// </summary>
        public Setup Setup;

        /// <summary>
        /// CmisSync icon in the task bar.
        /// It contains the main CmisSync menu.
        /// </summary>
        public StatusIcon StatusIcon;
        
        /// <summary>
        /// Small dialog showing some information about CmisSync.
        /// </summary>
        public About About;
        
        /// <summary>
        /// Small dialog showing setting about CmisSync.
        /// </summary>
        public Setting Setting;
        
        /// <summary>
        /// Window showing transmissions.
        /// </summary>
        public TransmissionWindow Transmission;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public UI() {
            Setup      = new Setup();
            About      = new About();
            Setting    = new Setting();
            Transmission   = new TransmissionWindow();
            Program.Controller.UIHasLoaded();
        }

        /// <summary>
        /// Run the CmisSync user interface.
        /// </summary>
        public void Run() {
            Application.ThreadException += delegate(Object sender, ThreadExceptionEventArgs args) {
                Logger.Fatal("UI Exception occured", args.Exception);
                Environment.Exit(-1);
            };
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += delegate(Object sender, UnhandledExceptionEventArgs args) {
                Logger.Fatal(string.Format("Unhandled Exception occured on object {0}", args.ExceptionObject.ToString()));
            };
            Application.Run(StatusIcon = new StatusIcon());
            StatusIcon.Dispose();
        }
    }
}