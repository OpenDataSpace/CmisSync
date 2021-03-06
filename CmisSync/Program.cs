//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="GRAU DATA AG">
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

[assembly: System.CLSCompliant(true)]
namespace CmisSync {
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Threading;

    using CmisSync.Lib;
    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Config;

    using log4net;
    using log4net.Config;

    /// <summary>
    /// Main Program.
    /// </summary>
    [CLSCompliant(false)]
    public class Program {
        /// <summary>
        /// User interface for CmisSync.
        /// </summary>
        public static UI UI;

        /// <summary>
        /// MVC controller.
        /// </summary>
        public static Controller Controller;

        /// <summary>
        /// Mutex checking whether CmisSync is already running or not.
        /// </summary>
        private static Mutex programMutex = new Mutex(false, "DataSpaceSync");

        /// <summary>
        /// Logging.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        /// <summary>
        /// The entry point of DataSpace Sync, where the program control starts and ends.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        [STAThread]
        public static void Main(string[] args) {
#if __MonoCS__
            Environment.SetEnvironmentVariable("MONO_XMLSERIALIZER_THS", "no");
#endif

#if __COCOA__
            MonoMac.AppKit.NSApplication.Init();
#endif
            bool firstRun = !File.Exists(ConfigManager.CurrentConfigFile);

            // Disable SSLv3 to avoid POODLE Attack
            ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol & ~SecurityProtocolType.Ssl3;

            ServicePointManager.CertificatePolicy = new CertPolicyHandler();

            // Migrate config.xml from past versions, if necessary.
            if (!firstRun) {
                ConfigMigration.Migrate();
            }

            FileInfo alternativeLog4NetConfigFile = new FileInfo(Path.Combine(Directory.GetParent(ConfigManager.CurrentConfigFile).FullName, "log4net.config"));
            if (alternativeLog4NetConfigFile.Exists) {
                log4net.Config.XmlConfigurator.ConfigureAndWatch(alternativeLog4NetConfigFile);
            } else {
                log4net.Config.XmlConfigurator.Configure(ConfigManager.CurrentConfig.GetLog4NetConfig());
            }

            Logger.Info("Starting. Version: " + CmisSync.Lib.Backend.Version);
            using (var listener = new CmisSync.Lib.Cmis.DotCMISLogListener()) {
                Trace.Listeners.Add(listener);
                if (args.Length != 0 && !args[0].Equals("start") &&
                Backend.Platform != PlatformID.MacOSX &&
                Backend.Platform != PlatformID.Win32NT) {
                    string n = Environment.NewLine;

                    Console.WriteLine(n + Properties_Resources.ApplicationName +
                    " is a collaboration and sharing tool that is" + n +
                    "designed to keep things simple and to stay out of your way." + n +
                    n +
                    "Version: " + CmisSync.Lib.Backend.Version + n +
                    "Copyright (C) 2016 GRAU DATA AG" + n +
                    "Copyright (C) 2010 Hylke Bons" + n +
                    "This program comes with ABSOLUTELY NO WARRANTY." + n +
                    n +
                    "This is free software, and you are welcome to redistribute it" + n +
                    "under certain conditions. Please read the GNU GPLv3 for details." + n +
                    n +
                    "Usage: dataspacesync [start|stop|restart]");
                    Environment.Exit(-1);
                }

                // Only allow one instance of CmisSync (on Windows)
                if (!programMutex.WaitOne(0, false)) {
                    Logger.Error(Properties_Resources.ApplicationName + " is already running.");
                    Environment.Exit(-1);
                }

                try {
                    CmisSync.Lib.Utils.EnsureNeededDependenciesAreAvailable();
                } catch (Exception e) {
                    string message = string.Format("Missing Dependency: {0}{1}{2}", e.Message, Environment.NewLine, e.StackTrace);
                    Logger.Error(message);
                    Console.Error.WriteLine(message);
                    Environment.Exit(-1);
                }

                // Increase the number of concurrent requests to each server,
                // as an unsatisfying workaround until this DotCMIS bug 632 is solved.
                // See https://github.com/nicolas-raoul/CmisSync/issues/140
                ServicePointManager.DefaultConnectionLimit = 1000;

                try {
                    Controller = new Controller();
                    Controller.Initialize(firstRun);

                    UI = new UI();
                    UI.Run();
                } catch (Exception e) {
                    Logger.Fatal("Exception in Program.Main", e);
                    Environment.Exit(-1);
                } finally {
                    if (Controller != null) {
                        Controller.Dispose();
                    }
                }
            }
        }
    }
}